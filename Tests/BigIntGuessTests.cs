using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TypeGuesser;
using TypeGuesser.Deciders;

namespace Tests
{
    class BigIntGuessTests
    {
        [Test]
        public void BigInt_TypeDeciderFactory()
        {
            var factory = new TypeDeciderFactory(new CultureInfo("en-US"));
            
            // The IDecideTypesForStrings for long should be DecimalTypeDecider
            Assert.IsTrue(factory.Dictionary[typeof(Int64)] is DecimalTypeDecider);
            Assert.IsTrue(factory.Dictionary[typeof(long)] is DecimalTypeDecider);
        }

        [Test]
        public void BigInt_Parse()
        {
            var decider = new DecimalTypeDecider(new CultureInfo("en-US"));

            Assert.AreEqual(100,decider.Parse("100"));
            Assert.AreEqual(9223372036854775807L,decider.Parse("9223372036854775807"));
            
        }


    }
}
