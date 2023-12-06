using NUnit.Framework;
using System.Globalization;
using TypeGuesser;
using TypeGuesser.Deciders;

namespace Tests;

internal sealed class BigIntGuessTests
{
    [Test]
    public void BigInt_TypeDeciderFactory()
    {
        var factory = new TypeDeciderFactory(new CultureInfo("en-US"));

        // The IDecideTypesForStrings for long should be DecimalTypeDecider
        Assert.That(factory.Dictionary[typeof(long)] is DecimalTypeDecider, Is.True);
    }

    [Test]
    public void BigInt_Parse()
    {
        var decider = new DecimalTypeDecider(new CultureInfo("en-US"));

        Assert.Multiple(() =>
        {
            Assert.That(decider.Parse("100"), Is.EqualTo(100));
            Assert.That(decider.Parse("9223372036854775807"), Is.EqualTo(9223372036854775807L));
        });

    }


}