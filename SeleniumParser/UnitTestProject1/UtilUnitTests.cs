using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumParser;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void MakeCsv_ReturnsExpectedCSVString_WhenGivenListOfInputs()
        {
            string result = Driver.ToCsv("a", "b", "c");
            string expectedResult = "a, b, c";

            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void MakeCsv_DoesNotReturnAStringWithATrailingComma_WhenGivenASingleInput()
        {
            string result = Driver.ToCsv("a");
            string expectedResult = "a";

            Assert.AreEqual(result, expectedResult);
        }
    }
}
