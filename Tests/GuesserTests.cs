using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TypeGuesser;
using TypeGuesser.Deciders;

namespace Tests
{
    /// <summary>
    /// <para>These tests cover the systems ability to compute a final <see cref="DatabaseTypeRequest"/> from a set of mixed data types.</para>
    /// 
    /// <para>Critically it covers fallback from one data type estimate to another based on new data e.g. if you see a "100" then a "1" then a "1.1"
    /// the final estimate should be decimal(4,1) to allow for both 100.0f and 1.1f.
    /// </para> 
    /// </summary>
    public class GuesserTests
    {

        [TestCase("5",typeof(int),"en-us",1,1,0,5)]
        [TestCase("true",typeof(bool),"en-us",4,0,0,true)]
        [TestCase("false",typeof(bool),"en-us",5,0,0,false)]
        [TestCase("0"    ,typeof(bool),"en-us",1,0,0,false)]
        [TestCase("1"    ,typeof(bool),"en-us",1,0,00,true)]
        [TestCase("j"    ,typeof(bool),"en-us",1,0,0,true)]
        [TestCase("y"    ,typeof(bool),"en-us",1,0,0,true)]
        [TestCase("n"    ,typeof(bool),"en-us",1,0,0,false)]
        [TestCase("0"    ,typeof(bool),"en-us",1,0,0,false)]
        [TestCase("1"    ,typeof(bool),"en-us",1,0,0,true)]
        [TestCase("T"    ,typeof(bool),"en-us",1,0,0,true)]
        [TestCase("F"    ,typeof(bool),"en-us",1,0,0,false)]
        [TestCase("x"    ,typeof(string),"en-us",1,0,0,"x")]
        [TestCase("100"  ,typeof(int),"en-us",3,3,0,100)]
        [TestCase("255"  ,typeof(int),"en-us",3,3,0,255)]
        [TestCase("abc"  ,typeof(string),"en-us",3,0,0,"abc")]
        [TestCase("ja"   ,typeof(bool),"de-de",2,0,0,true)]
        [TestCase("yes"  ,typeof(bool),"en-us",3,0,0,true)]
        [TestCase("n"    ,typeof(bool),"en-us",1,0,0,false)]
        [TestCase("nein" ,typeof(bool),"en-us",4,0,0,false)]
        [TestCase("no"   ,typeof(bool),"en-us",2,0,0,false)]
        [TestCase("t"    ,typeof(bool),"en-us",1,0,0,true)]
        [TestCase("f"    ,typeof(bool),"en-us",1,0,0,false)]
        [TestCase(".t."  ,typeof(bool),"en-us",3,0,0,true)]
        [TestCase(".f."  ,typeof(bool),"en-us",3,0,0,false)]
        [TestCase("9223372036854775807",typeof(decimal),"en-us",19,19,0,9223372036854775807L)]
        [TestCase("5000"  ,typeof(int),"en-us",4,4,0,5000)]
        [TestCase("5000.010", typeof(decimal), "en-us", 8, 4, 2, 5000.01)]
        [TestCase("5,123.001e-10", typeof(decimal), "en-us", 14, 0,13, 0.0000005123001)] //<=it's string length is 14 because this would be the string value we would need to represent
        [TestCase("5,000"  ,typeof(int),"en-us",5,4,0,5000)]
        [TestCase("5,000.01"  ,typeof(decimal),"en-us",8,4,2,5000.01)]
        [TestCase("5,000.01000"  ,typeof(decimal),"en-us",11,4,2,5000.01)]
        [TestCase("5.000,01000", typeof(decimal), "de-de", 11, 4, 2,5000.01)] //germans swap commas and dots
        [TestCase("5000010,000", typeof(int), "de-de", 11, 7, 0, 5000010)] //germans swap commas and dots
        [TestCase("5.000.000", typeof(string), "en-us", 9, 0, 0, "5.000.000")] //germans swap commas and dots so this is an illegal number
        [TestCase("5,000,000", typeof(string), "de-de", 9, 0, 0, "5,000,000")] //germans swap commas and dots so this is an illegal number
        [TestCase("5,000", typeof(int), "de-de", 5, 1,0,5)] //germans swap commas and dots

        public void Test_OneString_IsType(string guessFor, Type expectedGuess, string culture,int expectedStringLength, int expectedBefore,int expectedAfter,object expectedParseValue)
        {
            var cultureInfo = new CultureInfo(culture);
            var guesser = new Guesser(){Culture = cultureInfo};
            guesser.AdjustToCompensateForValue(guessFor);
            Assert.AreEqual(expectedGuess,guesser.Guess.CSharpType,"Guessed Type did not match");
            Assert.AreEqual(expectedStringLength,guesser.Guess.Width,"String length guessed didn't match");
            Assert.AreEqual(expectedBefore,guesser.Guess.Size.NumbersBeforeDecimalPlace,"BeforeDecimalPlace didn't match");
            Assert.AreEqual(expectedAfter,guesser.Guess.Size.NumbersAfterDecimalPlace,"AfterDecimalPlace didn't match");


            TypeDeciderFactory factory = new TypeDeciderFactory(cultureInfo);
            
            if (factory.IsSupported(guesser.Guess.CSharpType))
                Assert.AreEqual(expectedParseValue, factory.Create(guesser.Guess.CSharpType).Parse(guessFor));
            else
                Assert.AreEqual(expectedParseValue, guessFor);
            
            
        }

        [TestCase("en-us",new []{"5","10"},
            typeof(int),2,2,0)]
        [TestCase("en-us", new []{"5","10.1"},
            typeof(decimal),4,2,1)]
        [TestCase("en-us", new []{"5.1","5.000000000"},
            typeof(decimal),11,1,1)]
        public void Test_ManyString_IsType(string culture, string[] guessFor, Type expectedGuess,int expectedStringLength, int expectedBefore,int expectedAfter)
        {
            var cultureInfo = new CultureInfo(culture);
            var guesser = new Guesser(){Culture = cultureInfo};
            guesser.AdjustToCompensateForValues(guessFor);

            Assert.AreEqual(expectedGuess,guesser.Guess.CSharpType,"Guessed Type did not match");
            Assert.AreEqual(expectedStringLength,guesser.Guess.Width,"String length guessed didn't match");
            Assert.AreEqual(expectedBefore,guesser.Guess.Size.NumbersBeforeDecimalPlace,"BeforeDecimalPlace didn't match");
            Assert.AreEqual(expectedAfter,guesser.Guess.Size.NumbersAfterDecimalPlace,"AfterDecimalPlace didn't match");
        }

        [Test]
        public void ExampleUsage()
        {
            var guesser = new Guesser();
            guesser.AdjustToCompensateForValue("-12.211");
            var guess = guesser.Guess;
            
            Assert.AreEqual(typeof(decimal),guess.CSharpType);
            Assert.AreEqual(2, guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(3,guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(7,guess.Width);

                
            guesser = new Guesser();
            guesser.AdjustToCompensateForValue("1,000");
            guesser.AdjustToCompensateForValue("0.001");
            guess = guesser.Guess;
                
            Assert.AreEqual(typeof(decimal),guess.CSharpType);
            Assert.AreEqual(4,guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(3,guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(8,guess.Width);//?


            var someStrings = new []{"13:11:59", "9AM"};
            guesser = new Guesser();
            guesser.AdjustToCompensateForValues(someStrings);

            var parsed = someStrings.Select(guesser.Parse).ToArray();

            Assert.AreEqual(new TimeSpan(13, 11, 59), parsed[0]);
            Assert.AreEqual(new TimeSpan(9, 0, 0), parsed[1]);
        }

        [Test]
        public void TestGuesser_IntToFloat()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("12");
            
            Assert.AreEqual(typeof(int),t.Guess.CSharpType);
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(2, t.Guess.Size.NumbersBeforeDecimalPlace);

            t.AdjustToCompensateForValue("0.1");

            Assert.AreEqual(typeof(decimal), t.Guess.CSharpType);
            Assert.AreEqual(1, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(2, t.Guess.Size.NumbersBeforeDecimalPlace);
        }


        [Test]
        public void TestGuesser_IntToDate()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("12");

            Assert.AreEqual(typeof(int), t.Guess.CSharpType);
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(2, t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(2, t.Guess.Width);

            t.AdjustToCompensateForValue("2001-01-01");

            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(10, t.Guess.Width);
        }

        [Test]
        public void TestGuesser_decimal()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("1.5");
            t.AdjustToCompensateForValue("299.99");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(decimal),t.Guess.CSharpType);
        }

        [Test]
        public void TestGuesser_Int()
        {
            Guesser t = new Guesser();
            
            t.AdjustToCompensateForValue("0");
            Assert.AreEqual(typeof(bool), t.Guess.CSharpType);
            Assert.AreEqual(1, t.Guess.Width);

            t.AdjustToCompensateForValue("-0");
            Assert.AreEqual(typeof(int), t.Guess.CSharpType);
            Assert.AreEqual(2, t.Guess.Width);
            
            
            t.AdjustToCompensateForValue("15");
            t.AdjustToCompensateForValue("299");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(int),t.Guess.CSharpType);
        }

        /// <summary>
        /// Tests that we can fallback from an int guess to a long guess (which we will treat as decimal when parsing)
        /// </summary>
        [Test]
        public void TestGuesser_IntThenLong()
        {
            Guesser t = new Guesser();
            
            //we see an int
            t.AdjustToCompensateForValue("-100");

            //we guess the column contains ints
            Assert.AreEqual(typeof(int), t.Guess.CSharpType);
            Assert.AreEqual(4, t.Guess.Width);
            Assert.AreEqual(3, t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);

            //we see a long
            t.AdjustToCompensateForValue("9223372036854775807");

            //we change our estimate to the compatible estimate of 'decimal'
            Assert.AreEqual(typeof(decimal), t.Guess.CSharpType);
            Assert.AreEqual(19, t.Guess.Width);
            Assert.AreEqual(19, t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            
            //final estimate is decimal
            Assert.AreEqual(typeof(decimal),t.Guess.CSharpType);
        }

        [Test]
        public void TestGuesser_IntAnddecimal_MustUsedecimal()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("15");
            t.AdjustToCompensateForValue("29.9");
            t.AdjustToCompensateForValue("200");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(decimal),t.Guess.CSharpType);
        }
        [Test]
        public void TestGuesser_IntAnddecimal_MustUsedecimalThenString()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("15");
            t.AdjustToCompensateForValue("29.9");
            t.AdjustToCompensateForValue("200");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);
            
            Assert.AreEqual(typeof(decimal),t.Guess.CSharpType);
            Assert.AreEqual(3,t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(1,t.Guess.Size.NumbersAfterDecimalPlace);

            t.AdjustToCompensateForValue("D");
            Assert.AreEqual(typeof(string),t.Guess.CSharpType);
            Assert.AreEqual(5,t.Guess.Width);
        }
        [TestCase("20013001","yyyyddMM", 2001,1,30)]
        [TestCase("01302020","MMddyyyy", 2020,1,30)]
        public void DateTimeTypeDecider_ExplicitDateTimeFormat(string value,string format, int yy, int mm, int dd)
        {
            var decider = new DateTimeTypeDecider(CultureInfo.InvariantCulture);

            Assert.IsFalse(decider.IsAcceptableAsType(value, new DatabaseTypeRequest(typeof(DateTime),null,null)));

            decider.ExplicitDateFormats = new []{format };
            Assert.IsTrue(decider.IsAcceptableAsType(value, new DatabaseTypeRequest(typeof(DateTime),null,null)));

            Assert.AreEqual(new DateTime(yy,mm,dd),decider.Parse(value));

        }
        
        [Test]
        public void TestGuesser_DateTimeFromInt()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("01/01/2001");
            Assert.AreEqual(typeof(DateTime), t.Guess.CSharpType);

            t.AdjustToCompensateForValue("2013");
            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
        }

        //Tests system being happy to sign off in the orders bool=>int=>decimal but nothing else
        [TestCase("true", typeof(bool), "11", typeof(int))]
        [TestCase("1", typeof(bool), "1.1",typeof(decimal))]
        [TestCase("true", typeof(bool), "1.1", typeof(decimal))]
        public void TestGuesser_FallbackCompatible(string input1, Type expectedTypeAfterFirstInput, string input2, Type expectedTypeAfterSecondInput)
        {
            var t = new Guesser();
            t.AdjustToCompensateForValue(input1);
            
            Assert.AreEqual(expectedTypeAfterFirstInput,t.Guess.CSharpType);
            
            t.AdjustToCompensateForValue(input2);
            Assert.AreEqual(expectedTypeAfterSecondInput, t.Guess.CSharpType);
        }

        //Tests system being angry at having signed off on a bool=>int=>decimal then seeing a valid non string type (e.g. DateTime)
        //under these circumstances it should go directly to System.String
        [TestCase("1",typeof(bool),"2001-01-01")]
        [TestCase("true", typeof(bool), "2001-01-01")]
        [TestCase("1.1", typeof(decimal), "2001-01-01")]
        [TestCase("1.1", typeof(decimal), "10:00am")]
        [TestCase("2001-1-1", typeof(DateTime), "10:00am")]
        public void TestGuesser_FallbackIncompatible(string input1, Type expectedTypeAfterFirstInput, string input2)
        {
            var t = new Guesser();
            t.AdjustToCompensateForValue(input1);

            Assert.AreEqual(expectedTypeAfterFirstInput, t.Guess.CSharpType);

            t.AdjustToCompensateForValue(input2);
            Assert.AreEqual(typeof(string), t.Guess.CSharpType);

            //now check it in reverse just to be sure
            t = new Guesser();
            t.AdjustToCompensateForValue(input2);
            t.AdjustToCompensateForValue(input1);
            Assert.AreEqual(typeof(string),t.Guess.CSharpType);
        }

        [Test]
        public void TestGuesser_IntToDateTime()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("2013");
            t.AdjustToCompensateForValue("01/01/2001");
            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
        }

        [TestCase("fish",32)]
        [TestCase(32, "fish")]
        [TestCase("2001-01-01",2001)]
        [TestCase(2001, "2001-01-01")]
        [TestCase("2001", 2001)]
        [TestCase(2001, "2001")]
        public void TestGuesser_MixingTypes_ThrowsException(object o1, object o2)
        {
            //if we pass an hard type...
            //...then we don't accept strings anymore

            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(o1); 

            var ex = Assert.Throws<MixedTypingException>(() => t.AdjustToCompensateForValue(o2)); 
            StringAssert.Contains("mixed with untyped objects",ex.Message);
        }

        [Test]
        public void TestGuesser_DateTime()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("01/01/2001");
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(typeof(DateTime),t.Guess.CSharpType);
        }

        [TestCase("1. 01 ", typeof(DateTime))]
        [TestCase("1. 1 ", typeof(DateTime))]
        public void TestGuesser_DateTime_DodgyFormats(string input, Type expectedOutput)
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(input);
            Assert.AreEqual(expectedOutput, t.Guess.CSharpType);
        }

        [Test]
        public void TestGuesser_DateTime_English()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(GetCultureSpecificDate());
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(typeof(DateTime),t.Guess.CSharpType);
        }

        [Test]
        public void TestGuesser_DateTime_EnglishWithTime()
        {
            Guesser t = new Guesser();
            
            Console.WriteLine(CultureInfo.CurrentCulture.EnglishName);
            Console.WriteLine(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern);

            t.AdjustToCompensateForValue(GetCultureSpecificDate() + " 11:10");
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(typeof(DateTime),t.Guess.CSharpType);
        }

        private string GetCultureSpecificDate()
        {
            if (CultureInfo.CurrentCulture.EnglishName.Contains("United States"))
                return "01/23/2001";

            if (CultureInfo.CurrentCulture.EnglishName.Contains("Kingdom"))
                return "23/01/2001";

            Assert.Inconclusive("Did not have a good implementation of test date for culture " +CultureInfo.CurrentCulture.EnglishName);
            return null;
        }

        [Test]
        public void TestGuesser_DateTime_EnglishWithTimeAndAM()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(GetCultureSpecificDate()+" 11:10AM");
            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(typeof(DateTime),t.Guess.CSharpType);
        }

        [TestCase("01",2)]
        [TestCase("01.1", 4)]
        [TestCase("01.10", 5)]
        [TestCase("-01", 3)]
        [TestCase("-01.01", 6)]
        [TestCase(" -01.01", 7)]
        [TestCase("\t-01.01", 7)]
        [TestCase("\r\n-01.01", 8)]
        [TestCase("- 01.01", 7)]
        [TestCase(" -01.01 ", 8)]
        [TestCase("-01.01 ", 7)]
        [TestCase("--01", 4)]
        public void TestGuesser_PreeceedingZeroes(string input, int expectedLength)
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(input);
            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
            Assert.AreEqual(expectedLength, t.Guess.Width);
        }

        [Test]
        public void TestGuesser_PreeceedingZeroesAfterFloat()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("1.5");
            t.AdjustToCompensateForValue("00299.99");
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
        }
        [Test]
        public void TestGuesser_Negatives()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("-1");
            t.AdjustToCompensateForValue("-99.99");

            Assert.AreEqual(typeof(decimal),t.Guess.CSharpType);
            Assert.AreEqual(4,t.Guess.Size.Precision);
            Assert.AreEqual(2,t.Guess.Size.Scale);
        }


        [Test]
        public void TestGuesser_Doubles()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(299.99);
            
            Assert.AreEqual(typeof(double), t.Guess.CSharpType);

            Assert.AreEqual(2, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(3, t.Guess.Size.NumbersBeforeDecimalPlace);
        }

        [TestCase(" 1.01", typeof(decimal))]
        [TestCase(" 1.01 ", typeof(decimal))]
        [TestCase(" 1", typeof(int))]
        [TestCase(" true ",typeof(bool))]
        public void TestGuesser_Whitespace(string input, Type expectedType)
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(input);

            Assert.AreEqual(expectedType, t.Guess.CSharpType);
            Assert.AreEqual(input.Length,t.Guess.Width);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestGuesser_Bool(bool sendStringEquiv)
        {
            Guesser t = new Guesser();

            if (sendStringEquiv)
                t.AdjustToCompensateForValue("True");
            else
                t.AdjustToCompensateForValue(true);
            
            if (sendStringEquiv)
                t.AdjustToCompensateForValue("False");
            else
                t.AdjustToCompensateForValue(false);
            
            Assert.AreEqual(typeof(bool), t.Guess.CSharpType);

            t.AdjustToCompensateForValue(null);

            Assert.AreEqual(typeof(bool), t.Guess.CSharpType);

            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(0, t.Guess.Size.NumbersBeforeDecimalPlace);
        }

        [Test]
        public void TestGuesser_MixedIntTypes()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue((Int16)5);
            var ex = Assert.Throws<MixedTypingException>(()=>t.AdjustToCompensateForValue((Int32)1000));

            StringAssert.Contains("We were adjusting to compensate for object '1000' which is of Type 'System.Int32', we were previously passed a 'System.Int16' type",ex.Message);
        }
        [Test]
        public void TestGuesser_Int16s()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue((Int16)5);
            t.AdjustToCompensateForValue((Int16)10);
            t.AdjustToCompensateForValue((Int16)15);
            t.AdjustToCompensateForValue((Int16)30);
            t.AdjustToCompensateForValue((Int16)200);

            Assert.AreEqual(typeof(Int16), t.Guess.CSharpType);

            Assert.AreEqual(3, t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            

        }
        [Test]
        public void TestGuesser_Byte()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(new byte[5]);

            Assert.AreEqual(typeof(byte[]), t.Guess.CSharpType);

            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(0, t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.IsTrue(t.Guess.Size.IsEmpty);
        }


        [Test]
        public void TestGuesser_NumberOfDecimalPlaces()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("111111111.11111111111115");

            Assert.AreEqual(typeof(decimal), t.Guess.CSharpType);
            Assert.AreEqual(9, t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(14, t.Guess.Size.NumbersAfterDecimalPlace);
        }
        

        [Test]
        public void TestGuesser_TrailingZeroesFallbackToString()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("-111.000");
            
            Assert.AreEqual(typeof(int), t.Guess.CSharpType);
            Assert.AreEqual(3, t.Guess.Size.NumbersBeforeDecimalPlace);

            //even though they are trailing zeroes we still need this much space... there must be a reason why they are there right? (also makes it easier to go to string later if needed eh!)
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace); 
            
            t.AdjustToCompensateForValue("P");

            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
            Assert.AreEqual(8, t.Guess.Width);
        }

        [Test]
        public void TestGuesser_IntFloatString()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("-1000");
            Assert.AreEqual(typeof(int),t.Guess.CSharpType);

            t.AdjustToCompensateForValue("1.1");
            Assert.AreEqual(typeof(decimal),t.Guess.CSharpType);
            Assert.AreEqual(5,t.Guess.Size.Precision);
            Assert.AreEqual(1,t.Guess.Size.Scale);
            
            t.AdjustToCompensateForValue("A");
            Assert.AreEqual(typeof(string),t.Guess.CSharpType);
            Assert.AreEqual(6,t.Guess.Width);
        }

        [Test]
        public void TestGuesser_FallbackOntoVarcharFromFloat()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("15.5");
            t.AdjustToCompensateForValue("F");

            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
            Assert.AreEqual(4, t.Guess.Width);
        }
        [Test]
        public void TestGuesser_Time()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("12:30:00");

            Assert.AreEqual(typeof(TimeSpan), t.Guess.CSharpType);
            
        }

        [Test]
        public void TestGuesser_TimeNoSeconds()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("12:01");

            Assert.AreEqual(typeof(TimeSpan), t.Guess.CSharpType);
            
        }

        [Test]
        public void TestGuesser_TimeWithPM()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("1:01PM");

            Assert.AreEqual(typeof(TimeSpan), t.Guess.CSharpType);
            
        }
        [Test]
        public void TestGuesser_24Hour()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("23:01");

            Assert.AreEqual(typeof(TimeSpan), t.Guess.CSharpType);
            
        }
        [Test]
        public void TestGuesser_Midnight()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("00:00");

            Assert.AreEqual(typeof(TimeSpan), t.Guess.CSharpType);
            
        }
        [Test]
        public void TestGuesser_TimeObject()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(new TimeSpan(10,1,1));

            Assert.AreEqual(typeof(TimeSpan), t.Guess.CSharpType);
            
        }
        [Test]
        public void TestGuesser_MixedDateAndTime_FallbackToString()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue("09:01");
            Assert.AreEqual(typeof(TimeSpan), t.Guess.CSharpType);

            t.AdjustToCompensateForValue("2001-12-29 23:01");
            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
            Assert.AreEqual(16, t.Guess.Width);
        }

        [TestCase("1-1000")]
        public void TestGuesser_ValidDateStrings(string wierdDateString)
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(wierdDateString);
            Assert.AreEqual(typeof(DateTime), t.Guess.CSharpType);
        }

        [Test]
        public void TestGuesser_HardTypeFloats()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(1.1f);
            t.AdjustToCompensateForValue(100.01f);
            t.AdjustToCompensateForValue(10000f);

            Assert.AreEqual(typeof(float), t.Guess.CSharpType);
            Assert.AreEqual(2,t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(5, t.Guess.Size.NumbersBeforeDecimalPlace);
        }

        [Test]
        public void TestGuesser_HardTypeInts()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(1);
            t.AdjustToCompensateForValue(100);
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(10000);
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(int), t.Guess.CSharpType);
            Assert.AreEqual(0, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(5, t.Guess.Size.NumbersBeforeDecimalPlace);
        }


        [Test]
        public void TestGuesser_HardTypeDoubles()
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(1.1);
            t.AdjustToCompensateForValue(100.203);
            t.AdjustToCompensateForValue(100.20000);
            t.AdjustToCompensateForValue(null);
            t.AdjustToCompensateForValue(10000d);//<- d is required because Types must be homogenous
            t.AdjustToCompensateForValue(DBNull.Value);

            Assert.AreEqual(typeof(double), t.Guess.CSharpType);
            Assert.AreEqual(3, t.Guess.Size.NumbersAfterDecimalPlace);
            Assert.AreEqual(5, t.Guess.Size.NumbersBeforeDecimalPlace);
        }


        [TestCase("0.01",typeof(decimal),"A",4)]
        [TestCase("1234",typeof(int),"F",4)]
        [TestCase("false",typeof(bool), "M", 5)]
        [TestCase("2001-01-01",typeof(DateTime), "F", 27)]
        [TestCase("2001-01-01",typeof(DateTime), "FingersMcNultyFishBonesdlsiea", 29)]
        public void TestGuesser_FallbackOntoStringLength(string legitType, Type expectedLegitType, string str, int expectedLength)
        {
            Guesser t = new Guesser();
            
            //give it the legit hard typed value e.g. a date
            t.AdjustToCompensateForValue(legitType);
            Assert.AreEqual(expectedLegitType, t.Guess.CSharpType);

            //then give it a string
            t.AdjustToCompensateForValue(str);
            Assert.AreEqual(typeof(string), t.Guess.CSharpType);

            //the length should be the max of the length of the legit string and the string str
            Assert.AreEqual(expectedLength, t.Guess.Width);
            
        }

        [Test]
        [TestCase("-/-")]
        [TestCase("0/0")]
        [TestCase(".")]
        [TestCase("/")]
        [TestCase("-")]
        public void TestGuesser_RandomCrud(string randomCrud)
        {
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(randomCrud);
            Assert.AreEqual(typeof(string), t.Guess.CSharpType);
        }
        
        [Test]
        public void TestGuesser_ScientificNotation()
        {
            string val = "-4.10235746055587E-05"; //-0.0000410235746055587
            Guesser t = new Guesser();
            t.AdjustToCompensateForValue(val);
            Assert.AreEqual(typeof(decimal), t.Guess.CSharpType);
            
            //there is always 1 decimal place before point in order to allow for changing to string later on and retain a single leading 0.
            Assert.AreEqual(0, t.Guess.Size.NumbersBeforeDecimalPlace);
            Assert.AreEqual(19, t.Guess.Size.NumbersAfterDecimalPlace);
        }

        [TestCase("didn’t")]
        [TestCase("Æther")]
        [TestCase("乗")]
        public void Test_NonAscii_CharacterLength(string word)
        {
            var t = new Guesser();
            t.AdjustToCompensateForValue(word);
            
            //guesser should have picked up that it needs unicode
            Assert.IsTrue(t.Guess.Unicode);

            //in most DBMS
            Assert.AreEqual(t.Guess.Width,word.Length);

            //in the world of Oracle where you need varchar2(6) to store "It’s"
            t = new Guesser()
            {
                ExtraLengthPerNonAsciiCharacter = 3
            };
            t.AdjustToCompensateForValue(word);

            Assert.AreEqual(word.Length + 3, t.Guess.Width);
        }
    }
}
