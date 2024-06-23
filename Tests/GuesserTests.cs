using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TypeGuesser;
using TypeGuesser.Deciders;

namespace Tests;

/// <summary>
/// <para>These tests cover the systems ability to compute a final <see cref="DatabaseTypeRequest"/> from a set of mixed data types.</para>
/// 
/// <para>Critically it covers fallback from one data type estimate to another based on new data e.g. if you see a "100" then a "1" then a "1.1"
/// the final estimate should be decimal(4,1) to allow for both 100.0f and 1.1f.
/// </para>
/// </summary>
public sealed class GuesserTests
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
    public void Test_OneString_IsType(string guessFor, Type expectedGuess, string culture, int expectedStringLength,
        int expectedBefore, int expectedAfter, object expectedParseValue)
    {
        var cultureInfo = new CultureInfo(culture);
        var guesser = new Guesser {Culture = cultureInfo};
        guesser.AdjustToCompensateForValue(guessFor);
        Assert.Multiple(() =>
        {
            Assert.That(guesser.Guess.CSharpType, Is.EqualTo(expectedGuess), "Guessed Type did not match");
            Assert.That(guesser.Guess.Width, Is.EqualTo(expectedStringLength), "String length guessed didn't match");
            Assert.That(guesser.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(expectedBefore), "BeforeDecimalPlace didn't match");
            Assert.That(guesser.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(expectedAfter), "AfterDecimalPlace didn't match");
        });


        var factory = new TypeDeciderFactory(cultureInfo);

        Assert.That(factory.IsSupported(guesser.Guess.CSharpType)
                ? factory.Create(guesser.Guess.CSharpType).Parse(guessFor)
                : guessFor, Is.EqualTo(expectedParseValue));
    }

#pragma warning disable CA1861
    [TestCase("en-us",new []{"5","10"},
        typeof(int),2,2,0)]
    [TestCase("en-us", new []{"5","10.1"},
        typeof(decimal),4,2,1)]
    [TestCase("en-us", new []{"5.1","5.000000000"},
        typeof(decimal),11,1,1)]
#pragma warning restore CA1861
    public void Test_ManyString_IsType(string culture, string[] guessFor, Type expectedGuess,int expectedStringLength, int expectedBefore,int expectedAfter)
    {
        var cultureInfo = new CultureInfo(culture);
        var guesser = new Guesser {Culture = cultureInfo};
        guesser.AdjustToCompensateForValues(guessFor);

        Assert.Multiple(() =>
        {
            Assert.That(guesser.Guess.CSharpType, Is.EqualTo(expectedGuess), "Guessed Type did not match");
            Assert.That(guesser.Guess.Width, Is.EqualTo(expectedStringLength), "String length guessed didn't match");
            Assert.That(guesser.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(expectedBefore), "BeforeDecimalPlace didn't match");
            Assert.That(guesser.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(expectedAfter), "AfterDecimalPlace didn't match");
        });
    }

    [Test]
    public void ExampleUsage()
    {
        var guesser = new Guesser();
        guesser.AdjustToCompensateForValue("-12.211");
        var guess = guesser.Guess;

        Assert.Multiple(() =>
        {
            Assert.That(guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(2));
            Assert.That(guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(3));
            Assert.That(guess.Width, Is.EqualTo(7));
        });


        guesser = new Guesser();
        guesser.AdjustToCompensateForValue("1,000");
        guesser.AdjustToCompensateForValue("0.001");
        guess = guesser.Guess;

        Assert.Multiple(() =>
        {
            Assert.That(guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(4));
            Assert.That(guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(3));
            Assert.That(guess.Width, Is.EqualTo(8));//?
        });


        var someStrings = new []{"13:11:59", "9AM"};
        guesser = new Guesser();
        guesser.AdjustToCompensateForValues(someStrings);

        var parsed = someStrings.Select(guesser.Parse).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(parsed[0], Is.EqualTo(new TimeSpan(13, 11, 59)));
            Assert.That(parsed[1], Is.EqualTo(new TimeSpan(9, 0, 0)));
        });
    }

    [Test]
    public void TestGuesser_IntToFloat()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("12");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(2));
        });

        t.AdjustToCompensateForValue("0.1");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(1));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(2));
        });
    }


    [Test]
    public void TestGuesser_IntToDate()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("12");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(2));
            Assert.That(t.Guess.Width, Is.EqualTo(2));
        });

        t.AdjustToCompensateForValue("2001-01-01");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
            Assert.That(t.Guess.Width, Is.EqualTo(10));
        });
    }

    [Test]
    public void TestGuesser_decimal()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("1.5");
        t.AdjustToCompensateForValue("299.99");
        t.AdjustToCompensateForValue(null);
        t.AdjustToCompensateForValue(DBNull.Value);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
    }

    [Test]
    public void TestGuesser_Int()
    {
        var t = new Guesser();

        t.AdjustToCompensateForValue("0");
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(bool)));
            Assert.That(t.Guess.Width, Is.EqualTo(1));
        });

        t.AdjustToCompensateForValue("-0");
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));
            Assert.That(t.Guess.Width, Is.EqualTo(2));
        });


        t.AdjustToCompensateForValue("15");
        t.AdjustToCompensateForValue("299");
        t.AdjustToCompensateForValue(null);
        t.AdjustToCompensateForValue(DBNull.Value);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));
    }

    /// <summary>
    /// Tests that we can fallback from an int guess to a long guess (which we will treat as decimal when parsing)
    /// </summary>
    [Test]
    public void TestGuesser_IntThenLong()
    {
        var t = new Guesser();

        //we see an int
        t.AdjustToCompensateForValue("-100");

        Assert.Multiple(() =>
        {
            //we guess the column contains ints
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));
            Assert.That(t.Guess.Width, Is.EqualTo(4));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(3));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
        });

        //we see a long
        t.AdjustToCompensateForValue("9223372036854775807");

        Assert.Multiple(() =>
        {
            //we change our estimate to the compatible estimate of 'decimal'
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(t.Guess.Width, Is.EqualTo(19));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(19));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
        });

        //final estimate is decimal
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
    }

    [Test]
    public void TestGuesser_IntAndDecimal_MustUseDecimal()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("15");
        t.AdjustToCompensateForValue("29.9");
        t.AdjustToCompensateForValue("200");
        t.AdjustToCompensateForValue(null);
        t.AdjustToCompensateForValue(DBNull.Value);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
    }
    [Test]
    public void TestGuesser_IntAndDecimal_MustUseDecimalThenString()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("15");
        t.AdjustToCompensateForValue("29.9");
        t.AdjustToCompensateForValue("200");
        t.AdjustToCompensateForValue(null);
        t.AdjustToCompensateForValue(DBNull.Value);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(3));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(1));
        });

        t.AdjustToCompensateForValue("D");
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
            Assert.That(t.Guess.Width, Is.EqualTo(5));
        });
    }

    /// <summary>
    /// Tests the <see cref="Guesser"/> and <see cref="DateTimeTypeDecider"/> classes to ensure they normally do not treat
    /// these things as dates but will do when told (e.g. by <see cref="GuessSettings.ExplicitDateFormats"/>)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="format"></param>
    /// <param name="yy"></param>
    /// <param name="mm"></param>
    /// <param name="dd"></param>
    [TestCase("20013001","yyyyddMM", 2001,1,30)]
    [TestCase("01302020","MMddyyyy", 2020,1,30)]
    public void DateTimeTypeDecider_ExplicitDateTimeFormat(string value,string format, int yy, int mm, int dd)
    {
        var decider = new DateTimeTypeDecider(CultureInfo.InvariantCulture);

        Assert.That(decider.IsAcceptableAsType(value, new DatabaseTypeRequest(typeof(DateTime))), Is.False);

        decider.Settings.ExplicitDateFormats = [format];
        Assert.Multiple(() =>
        {
            Assert.That(decider.IsAcceptableAsType(value, new DatabaseTypeRequest(typeof(DateTime))), Is.True);

            Assert.That(decider.Parse(value), Is.EqualTo(new DateTime(yy, mm, dd)));
        });

        var g = new Guesser();
        g.AdjustToCompensateForValue(value);
        Assert.That(g.Guess.CSharpType == typeof(int) || g.Guess.CSharpType == typeof(string) /*0 prefixed numbers are usually treated as strings*/, Is.True);


        var g2 = new Guesser
        {
            Settings =
            {
                ExplicitDateFormats = [format]
            }
        };
        g2.AdjustToCompensateForValue(value);
        Assert.That(g2.Guess.CSharpType, Is.EqualTo(typeof(DateTime)));

    }

    [Test]
    public void TestGuesser_DateTimeFromInt()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("01/01/2001");
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(DateTime)));

        t.AdjustToCompensateForValue("2013");
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
    }

    //Tests system being happy to sign off in the orders bool=>int=>decimal but nothing else
    [TestCase("true", typeof(bool), "11", typeof(int))]
    [TestCase("1", typeof(bool), "1.1",typeof(decimal))]
    [TestCase("true", typeof(bool), "1.1", typeof(decimal))]
    public void TestGuesser_FallbackCompatible(string input1, Type expectedTypeAfterFirstInput, string input2, Type expectedTypeAfterSecondInput)
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(input1);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(expectedTypeAfterFirstInput));

        t.AdjustToCompensateForValue(input2);
        Assert.That(t.Guess.CSharpType, Is.EqualTo(expectedTypeAfterSecondInput));
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

        Assert.That(t.Guess.CSharpType, Is.EqualTo(expectedTypeAfterFirstInput));

        t.AdjustToCompensateForValue(input2);
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));

        //now check it in reverse just to be sure
        t = new Guesser();
        t.AdjustToCompensateForValue(input2);
        t.AdjustToCompensateForValue(input1);
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void TestGuesser_IntToDateTime()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("2013");
        t.AdjustToCompensateForValue("01/01/2001");
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
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
        //...then we don't accept strings any more

        var t = new Guesser();
        t.AdjustToCompensateForValue(o1);

        var ex = Assert.Throws<MixedTypingException>(() => t.AdjustToCompensateForValue(o2));
        Assert.That(ex?.Message, Does.Contain("mixed with untyped objects"));
    }

    [Test]
    public void TestGuesser_DateTime()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("01/01/2001");
        t.AdjustToCompensateForValue(null);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(DateTime)));
    }

    [TestCase("1. 01 ", typeof(DateTime))]
    [TestCase("1. 1 ", typeof(DateTime))]
    public void TestGuesser_DateTime_DodgyFormats(string input, Type expectedOutput)
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(input);
        Assert.That(t.Guess.CSharpType, Is.EqualTo(expectedOutput));
    }

    [Test]
    public void TestGuesser_DateTime_English()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(GetCultureSpecificDate());
        t.AdjustToCompensateForValue(null);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(DateTime)));
    }

    [Test]
    public void TestGuesser_DateTime_EnglishWithTime()
    {
        var t = new Guesser();

        Console.WriteLine(CultureInfo.CurrentCulture.EnglishName);
        Console.WriteLine(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern);

        t.AdjustToCompensateForValue($"{GetCultureSpecificDate()} 11:10");
        t.AdjustToCompensateForValue(null);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(DateTime)));
    }

    private static string GetCultureSpecificDate()
    {
        if (CultureInfo.CurrentCulture.EnglishName.Contains("United States"))
            return "01/23/2001";

        if (CultureInfo.CurrentCulture.EnglishName.Contains("Kingdom"))
            return "23/01/2001";

        Assert.Inconclusive(
            $"Did not have a good implementation of test date for culture {CultureInfo.CurrentCulture.EnglishName}");
        return null;
    }

    [Test]
    public void TestGuesser_DateTime_EnglishWithTimeAndAM()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue($"{GetCultureSpecificDate()} 11:10AM");
        t.AdjustToCompensateForValue(null);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(DateTime)));
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
    public void TestGuesser_PrecedingZeroes(string input, int expectedLength)
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(input);
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
            Assert.That(t.Guess.Width, Is.EqualTo(expectedLength));
        });
    }

    [Test]
    public void TestGuesser_PrecedingZeroesAfterFloat()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("1.5");
        t.AdjustToCompensateForValue("00299.99");
        t.AdjustToCompensateForValue(null);
        t.AdjustToCompensateForValue(DBNull.Value);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
    }
    [Test]
    public void TestGuesser_Negatives()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("-1");
        t.AdjustToCompensateForValue("-99.99");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(t.Guess.Size.Precision, Is.EqualTo(4));
            Assert.That(t.Guess.Size.Scale, Is.EqualTo(2));
        });
    }


    [Test]
    public void TestGuesser_Doubles()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(299.99);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(double)));

            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(2));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(3));
        });
    }

    [TestCase(" 1.01", typeof(decimal))]
    [TestCase(" 1.01 ", typeof(decimal))]
    [TestCase(" 1", typeof(int))]
    [TestCase(" true ",typeof(bool))]
    public void TestGuesser_Whitespace(string input, Type expectedType)
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(input);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(expectedType));
            Assert.That(t.Guess.Width, Is.EqualTo(input.Length));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestGuesser_Bool(bool sendStringEquiv)
    {
        var t = new Guesser();

        if (sendStringEquiv)
            t.AdjustToCompensateForValue("True");
        else
            t.AdjustToCompensateForValue(true);

        if (sendStringEquiv)
            t.AdjustToCompensateForValue("False");
        else
            t.AdjustToCompensateForValue(false);

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(bool)));

        t.AdjustToCompensateForValue(null);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(bool)));

            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(0));
        });
    }

    [Test]
    public void TestGuesser_MixedIntTypes()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue((short)5);
        var ex = Assert.Throws<MixedTypingException>(()=>t.AdjustToCompensateForValue(1000));

        Assert.That(ex?.Message, Does.Contain("We were adjusting to compensate for object '1000' which is of Type 'System.Int32', we were previously passed a 'System.Int16' type"));
    }
    [Test]
    public void TestGuesser_Int16s()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue((short)5);
        t.AdjustToCompensateForValue((short)10);
        t.AdjustToCompensateForValue((short)15);
        t.AdjustToCompensateForValue((short)30);
        t.AdjustToCompensateForValue((short)200);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(short)));

            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(3));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
        });


    }
    [Test]
    public void TestGuesser_Byte()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(new byte[5]);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(byte[])));

            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(0));
        });
        Assert.That(t.Guess.Size.IsEmpty, Is.True);
    }


    [Test]
    public void TestGuesser_NumberOfDecimalPlaces()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("111111111.11111111111115");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(9));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(14));
        });
    }


    [Test]
    public void TestGuesser_TrailingZeroesFallbackToString()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("-111.000");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(3));

            //even though they are trailing zeroes we still need this much space... there must be a reason why they are there right? (also makes it easier to go to string later if needed eh!)
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
        });

        t.AdjustToCompensateForValue("P");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
            Assert.That(t.Guess.Width, Is.EqualTo(8));
        });
    }

    [Test]
    public void TestGuesser_IntFloatString()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("-1000");
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));

        t.AdjustToCompensateForValue("1.1");
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));
            Assert.That(t.Guess.Size.Precision, Is.EqualTo(5));
            Assert.That(t.Guess.Size.Scale, Is.EqualTo(1));
        });

        t.AdjustToCompensateForValue("A");
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
            Assert.That(t.Guess.Width, Is.EqualTo(6));
        });
    }

    [Test]
    public void TestGuesser_FallbackOntoVarcharFromFloat()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("15.5");
        t.AdjustToCompensateForValue("F");

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
            Assert.That(t.Guess.Width, Is.EqualTo(4));
        });
    }
    [Test]
    public void TestGuesser_Time()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("12:30:00");

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(TimeSpan)));

    }

    [Test]
    public void TestGuesser_TimeNoSeconds()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("12:01");

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(TimeSpan)));

    }

    [Test]
    public void TestGuesser_TimeWithPM()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("1:01PM");

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(TimeSpan)));

    }
    [Test]
    public void TestGuesser_24Hour()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("23:01");

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(TimeSpan)));

    }
    [Test]
    public void TestGuesser_Midnight()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("00:00");

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(TimeSpan)));

    }
    [Test]
    public void TestGuesser_TimeObject()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(new TimeSpan(10,1,1));

        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(TimeSpan)));

    }
    [Test]
    public void TestGuesser_MixedDateAndTime_FallbackToString()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue("09:01");
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(TimeSpan)));

        t.AdjustToCompensateForValue("2001-12-29 23:01");
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
            Assert.That(t.Guess.Width, Is.EqualTo(16));
        });
    }

    [TestCase("1-1000")]
    public void TestGuesser_ValidDateStrings(string weirdDateString)
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(weirdDateString);
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(DateTime)));
    }

    [Test]
    public void TestGuesser_HardTypeFloats()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(1.1f);
        t.AdjustToCompensateForValue(100.01f);
        t.AdjustToCompensateForValue(10000f);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(float)));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(2));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(5));
        });
    }

    [Test]
    public void TestGuesser_HardTypeInts()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(1);
        t.AdjustToCompensateForValue(100);
        t.AdjustToCompensateForValue(null);
        t.AdjustToCompensateForValue(10000);
        t.AdjustToCompensateForValue(DBNull.Value);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(int)));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(0));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(5));
        });
    }


    [Test]
    public void TestGuesser_HardTypeDoubles()
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(1.1);
        t.AdjustToCompensateForValue(100.203);
        t.AdjustToCompensateForValue(100.20000);
        t.AdjustToCompensateForValue(null);
        t.AdjustToCompensateForValue(10000d);//<- d is required because Types must be homogenous
        t.AdjustToCompensateForValue(DBNull.Value);

        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(double)));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(3));
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(5));
        });
    }


    [TestCase("0.01",typeof(decimal),"A",4)]
    [TestCase("1234",typeof(int),"F",4)]
    [TestCase("false",typeof(bool), "M", 5)]
    [TestCase("2001-01-01",typeof(DateTime), "F", 27)]
    [TestCase("2001-01-01",typeof(DateTime), "FingersMcNultyFishBonesdlsiea", 29)]
    public void TestGuesser_FallbackOntoStringLength(string legitType, Type expectedLegitType, string str, int expectedLength)
    {
        var t = new Guesser();

        //give it the legit hard typed value e.g. a date
        t.AdjustToCompensateForValue(legitType);
        Assert.That(t.Guess.CSharpType, Is.EqualTo(expectedLegitType));

        //then give it a string
        t.AdjustToCompensateForValue(str);
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));

            //the length should be the max of the length of the legit string and the string str
            Assert.That(t.Guess.Width, Is.EqualTo(expectedLength));
        });

    }

    [Test]
    [TestCase("-/-")]
    [TestCase("0/0")]
    [TestCase(".")]
    [TestCase("/")]
    [TestCase("-")]
    public void TestGuesser_RandomCrud(string randomCrud)
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(randomCrud);
        Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void TestGuesser_ScientificNotation()
    {
        const string val = "-4.10235746055587E-05"; //-0.0000410235746055587
        var t = new Guesser();
        t.AdjustToCompensateForValue(val);
        Assert.Multiple(() =>
        {
            Assert.That(t.Guess.CSharpType, Is.EqualTo(typeof(decimal)));

            //there is always 1 decimal place before point in order to allow for changing to string later on and retain a single leading 0.
            Assert.That(t.Guess.Size.NumbersBeforeDecimalPlace, Is.EqualTo(0));
            Assert.That(t.Guess.Size.NumbersAfterDecimalPlace, Is.EqualTo(19));
        });
    }

    [TestCase("didn’t")]
    [TestCase("Æther")]
    [TestCase("乗")]
    public void Test_NonAscii_CharacterLength(string word)
    {
        var t = new Guesser();
        t.AdjustToCompensateForValue(word);

        Assert.Multiple(() =>
        {
            //guesser should have picked up that it needs unicode
            Assert.That(t.Guess.Unicode, Is.True);

            //in most DBMS
            Assert.That(word, Has.Length.EqualTo(t.Guess.Width));
        });

        //in the world of Oracle where you need varchar2(6) to store "It’s"
        t = new Guesser
        {
            ExtraLengthPerNonAsciiCharacter = 3
        };
        t.AdjustToCompensateForValue(word);

        Assert.That(t.Guess.Width, Is.EqualTo(word.Length + 3));
    }
}