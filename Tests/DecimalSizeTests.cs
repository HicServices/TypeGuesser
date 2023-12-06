using NUnit.Framework;
using TypeGuesser;

namespace Tests;

internal sealed class DecimalSizeTests
{
    [Test]
    public void Test_DecimalSize_Empty()
    {
        var empty = new DecimalSize();

        Assert.Multiple(() =>
        {
            Assert.That(empty.NumbersAfterDecimalPlace, Is.EqualTo(0));
            Assert.That(empty.NumbersBeforeDecimalPlace, Is.EqualTo(0));

            Assert.That(empty.Precision, Is.EqualTo(0));
            Assert.That(empty.Scale, Is.EqualTo(0));
        });

        Assert.That(empty.IsEmpty, Is.True);
    }


    [Test]
    public void Test_DecimalSize_Equality()
    {
        Assert.Multiple(static () =>
        {
#pragma warning disable NUnit2009 // The same value has been provided as both the actual and the expected argument
            Assert.That(new DecimalSize(), Is.EqualTo(new DecimalSize()));
#pragma warning restore NUnit2009 // The same value has been provided as both the actual and the expected argument
            Assert.That(new DecimalSize { NumbersAfterDecimalPlace = 0 }, Is.EqualTo(new DecimalSize()));
            Assert.That(new DecimalSize { NumbersAfterDecimalPlace = 0, NumbersBeforeDecimalPlace = 0 }, Is.EqualTo(new DecimalSize()));
#pragma warning disable NUnit2009 // The same value has been provided as both the actual and the expected argument - testing for consistency
            Assert.That(new DecimalSize(3, 4), Is.EqualTo(new DecimalSize(3, 4)));
        });
        Assert.Multiple(static () =>
        {
#pragma warning restore NUnit2009 // The same value has been provided as both the actual and the expected argument

#pragma warning disable NUnit2009 // The same value has been provided as both the actual and the expected argument - tests for consistency
            Assert.That(new DecimalSize().GetHashCode(), Is.EqualTo(new DecimalSize().GetHashCode()));
#pragma warning restore NUnit2009 // The same value has been provided as both the actual and the expected argument
            Assert.That(new DecimalSize { NumbersAfterDecimalPlace = 0 }.GetHashCode(), Is.EqualTo(new DecimalSize().GetHashCode()));
            Assert.That(new DecimalSize { NumbersAfterDecimalPlace = 0, NumbersBeforeDecimalPlace = 0 }.GetHashCode(), Is.EqualTo(new DecimalSize().GetHashCode()));
            Assert.That(new DecimalSize { NumbersAfterDecimalPlace = 4, NumbersBeforeDecimalPlace = 3 }.GetHashCode(), Is.EqualTo(new DecimalSize(3, 4).GetHashCode()));
        });
    }

    [Test]
    public void Test_DecimalSize_NoFraction()
    {
        //decimal(5,0)
        var size = new DecimalSize(5,0);

        Assert.Multiple(() =>
        {
            Assert.That(size.Precision, Is.EqualTo(5));
            Assert.That(size.Scale, Is.EqualTo(0));
        });

        Assert.That(size.IsEmpty, Is.False);
    }
    [Test]
    public void Test_DecimalSize_SomeFraction()
    {
        //decimal(7,2)
        var size = new DecimalSize(5,2);

        Assert.Multiple(() =>
        {
            Assert.That(size.Precision, Is.EqualTo(7));
            Assert.That(size.Scale, Is.EqualTo(2));
        });

        Assert.That(size.IsEmpty, Is.False);
    }


    [Test]
    public void Test_DecimalSize_Combine()
    {
        //decimal(3,0)
        var size1 = new DecimalSize(3,0);
        Assert.Multiple(() =>
        {
            Assert.That(size1.Precision, Is.EqualTo(3));
            Assert.That(size1.Scale, Is.EqualTo(0));
        });

        //decimal(5,4)
        var size2 = new DecimalSize(1,4);
        Assert.Multiple(() =>
        {
            Assert.That(size2.Precision, Is.EqualTo(5));
            Assert.That(size2.Scale, Is.EqualTo(4));
        });


        var combined = DecimalSize.Combine(size1,size2);

        Assert.Multiple(() =>
        {
            Assert.That(combined.NumbersBeforeDecimalPlace, Is.EqualTo(3));
            Assert.That(combined.NumbersAfterDecimalPlace, Is.EqualTo(4));

            //decimal(7,4)
            Assert.That(combined.Precision, Is.EqualTo(7));
            Assert.That(combined.Scale, Is.EqualTo(4));
        });
    }
}