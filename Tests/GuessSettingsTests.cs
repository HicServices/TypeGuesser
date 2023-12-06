using System.Globalization;
using NUnit.Framework;
using TypeGuesser;
using TypeGuesser.Deciders;

namespace Tests;

internal class GuessSettingsTests
{
    [TestCase("Y")]
    [TestCase("N")]
    [TestCase("J")]
    [TestCase("F")]
    [TestCase("T")]
    public void Guess_TF_Settings(string candidate)
    {
        //default is true
        Assert.That(GuessSettingsFactory.Defaults.CharCanBeBoolean, Is.True);

        var decider =  new BoolTypeDecider(CultureInfo.CurrentCulture);

        Assert.Multiple(() =>
        {
            //T = True
            Assert.That(decider.IsAcceptableAsType(candidate, null), Is.True);
            Assert.That(decider.IsAcceptableAsType("1", null), Is.True);
        });

        decider.Settings.CharCanBeBoolean = false;
        Assert.Multiple(() =>
        {
            Assert.That(decider.IsAcceptableAsType(candidate, null), Is.False);
            Assert.That(decider.IsAcceptableAsType("1", null), Is.True); //setting does not affect 1/0
        });
    }

    [Test]
    public void Guess_TF_Factory()
    {
        const string candidate = "T";

        //default is true
        Assert.That(GuessSettingsFactory.Defaults.CharCanBeBoolean, Is.True);

        var factory =  new TypeDeciderFactory(CultureInfo.CurrentCulture);
        var deciderOld = factory.Create(typeof(bool));

        Assert.Multiple(() =>
        {
            //T = True
            Assert.That(deciderOld.IsAcceptableAsType(candidate, null), Is.True);
            Assert.That(deciderOld.IsAcceptableAsType("1", null), Is.True);
        });

        factory.Settings.CharCanBeBoolean = false;
        var deciderNew = factory.Create(typeof(bool));

        Assert.Multiple(() =>
        {
            Assert.That(ReferenceEquals(deciderOld, deciderNew), Is.False, "Factory Create created a reference to the same old instance! not a fresh one");
            Assert.That(deciderOld.Settings.CharCanBeBoolean, Is.True);
            Assert.That(deciderNew.Settings.CharCanBeBoolean, Is.False);

            Assert.That(deciderNew.IsAcceptableAsType(candidate, null), Is.False);
            Assert.That(deciderNew.IsAcceptableAsType("1", null), Is.True); //setting does not affect 1/0
        });
    }
    [Test]
    public void Guess_TF_Guesser()
    {
        const string candidate = "T";

        //default is true
        Assert.That(GuessSettingsFactory.Defaults.CharCanBeBoolean, Is.True);

        //start with a guesser
        var guesser =  new Guesser();

        //give the guesser "Y" which is acceptable as bool
        guesser.AdjustToCompensateForValue(candidate);

        //so bool should be the guess for "Y"
        Assert.That(guesser.Guess.CSharpType, Is.EqualTo(typeof(bool)));

        //change the guesser settings so "Y" is no longer acceptable as bool
        guesser.Settings.CharCanBeBoolean = false;
        Assert.That(guesser.Guess.CSharpType, Is.EqualTo(typeof(bool))); //guess is only re evaluated on calls to Adjust

        //Guess again!
        guesser.AdjustToCompensateForValue(candidate);
        Assert.That(guesser.Guess.CSharpType, Is.EqualTo(typeof(string)), "Guess should be string after the next evaluation");

    }

    [Test]
    public void Test_GuessSettingsFactory_Defaults()
    {
        //default is true
        Assert.That(GuessSettingsFactory.Defaults.CharCanBeBoolean, Is.True);
        var instance = GuessSettingsFactory.Create();
        Assert.Multiple(() =>
        {
            Assert.That(instance.CharCanBeBoolean, Is.True);

            Assert.That(instance, Is.Not.SameAs(GuessSettingsFactory.Defaults));
        });

        //changing static defaults
        GuessSettingsFactory.Defaults.CharCanBeBoolean = false;

        try
        {
            //should change the result of Create to the new default
            Assert.That(GuessSettingsFactory.Create().CharCanBeBoolean, Is.False);

            var decider = new DecimalTypeDecider(CultureInfo.CurrentCulture);
            Assert.That(decider.Settings.CharCanBeBoolean, Is.False);
        }
        finally
        {
            //set the static default back to not interfere with other tests
            GuessSettingsFactory.Defaults.CharCanBeBoolean = true;
        }
    }
}