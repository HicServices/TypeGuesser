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
        Assert.IsTrue(GuessSettingsFactory.Defaults.CharCanBeBoolean);

        var decider =  new BoolTypeDecider(CultureInfo.CurrentCulture);
            
        //T = True
        Assert.IsTrue(decider.IsAcceptableAsType(candidate,null));
        Assert.IsTrue(decider.IsAcceptableAsType("1",null));

        decider.Settings.CharCanBeBoolean = false;
        Assert.IsFalse(decider.IsAcceptableAsType(candidate,null));
        Assert.IsTrue(decider.IsAcceptableAsType("1",null)); //setting does not affect 1/0
    }

    [Test]
    public void Guess_TF_Factory()
    {
        const string candidate = "T";

        //default is true
        Assert.IsTrue(GuessSettingsFactory.Defaults.CharCanBeBoolean);

        var factory =  new TypeDeciderFactory(CultureInfo.CurrentCulture);
        var deciderOld = factory.Create(typeof(bool));
            
        //T = True
        Assert.IsTrue(deciderOld.IsAcceptableAsType(candidate,null));
        Assert.IsTrue(deciderOld.IsAcceptableAsType("1",null));

        factory.Settings.CharCanBeBoolean = false;
        var deciderNew = factory.Create(typeof(bool));

        Assert.IsFalse(ReferenceEquals(deciderOld,deciderNew), "Factory Create created a reference to the same old instance! not a fresh one");
        Assert.IsTrue(deciderOld.Settings.CharCanBeBoolean);
        Assert.IsFalse(deciderNew.Settings.CharCanBeBoolean);

        Assert.IsFalse(deciderNew.IsAcceptableAsType(candidate,null));
        Assert.IsTrue(deciderNew.IsAcceptableAsType("1",null)); //setting does not affect 1/0
    }
    [Test]
    public void Guess_TF_Guesser()
    {
        const string candidate = "T";

        //default is true
        Assert.IsTrue(GuessSettingsFactory.Defaults.CharCanBeBoolean);

        //start with a guesser
        var guesser =  new Guesser();

        //give the guesser "Y" which is acceptable as bool
        guesser.AdjustToCompensateForValue(candidate);

        //so bool should be the guess for "Y"
        Assert.AreEqual(typeof(bool),guesser.Guess.CSharpType);

        //change the guesser settings so "Y" is no longer acceptable as bool
        guesser.Settings.CharCanBeBoolean = false;
        Assert.AreEqual(typeof(bool),guesser.Guess.CSharpType); //guess is only re evaluated on calls to Adjust

        //Guess again!
        guesser.AdjustToCompensateForValue(candidate);
        Assert.AreEqual(typeof(string),guesser.Guess.CSharpType,"Guess should be string after the next evaluation");

    }
        
    [Test]
    public void Test_GuessSettingsFactory_Defaults()
    {
        //default is true
        Assert.IsTrue(GuessSettingsFactory.Defaults.CharCanBeBoolean);
        var f = new GuessSettingsFactory();
        var instance = GuessSettingsFactory.Create();
        Assert.IsTrue(instance.CharCanBeBoolean);

        Assert.IsFalse(instance == GuessSettingsFactory.Defaults);

        //changing static defaults
        GuessSettingsFactory.Defaults.CharCanBeBoolean = false;

        try
        {
            //should change the result of Create to the new default
            Assert.IsFalse(GuessSettingsFactory.Create().CharCanBeBoolean);

            var decider = new DecimalTypeDecider(CultureInfo.CurrentCulture);
            Assert.IsFalse(decider.Settings.CharCanBeBoolean);
        }
        finally
        {
            //set the static default back to not interfere with other tests
            GuessSettingsFactory.Defaults.CharCanBeBoolean = true;
        }
    }
}