namespace KJU.Tests.Input
{
    using KJU.Core.Input;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringLocationTests
    {
        [TestMethod]
        public void TestToString()
        {
            var expected = "10";
            var actual = new StringLocation(10).ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParams()
        {
            var expected = 10;
            var actual = new StringLocation(10).Position;
            Assert.AreEqual(expected, actual);
        }
    }
}
