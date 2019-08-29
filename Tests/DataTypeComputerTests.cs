using System;
using System.Globalization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using TypeGuesser;

namespace Tests
{
    public class DataTypeComputerTests
    {
        [TestCase("5","en-US",typeof(int))]
        public void Test_StringValueIsType_EnUs(string input,string culture, Type expectedType)
        {
            var cultureInfo = new CultureInfo(culture);

            var computer = new DataTypeComputer(){Culture = cultureInfo};
            computer.AdjustToCompensateForValue(input);
            Assert.AreEqual(expectedType,computer.CurrentEstimate);
        }
    }
}
