using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumParser;

namespace UnitTestProject1
{
    [TestClass]
    public class BsrRankUnitTests
    {
        [TestMethod]
        public void RemoveCommas_RemovesCommasFromString_ThatContainsCommas()
        {
            var stringValue = "A string, that contains, some commas,";

            var result = BsrRank.RemoveCommas(stringValue);

            var expectedResult = "A string that contains some commas";

            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void RemoveCommas_DoesNothingToInputString_WhichDoesNotContainCommas()
        {
            var stringValue = "A string that contains no commas";

            var result = BsrRank.RemoveCommas(stringValue);

            var expectedResult = "A string that contains no commas";

            Assert.AreEqual(result, expectedResult);
        }
    }
}
