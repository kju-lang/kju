namespace KJU.Tests.Input
{
    using KJU.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringLocationTests
    {
        [TestMethod]
        public void TestToString()
        {
            KJU.Core.Input.ILocation location = new KJU.Core.Input.StringLocation(10);
            Assert.AreEqual("10", location.ToString());
        }

        [TestMethod]
        public void TestParams()
        {
            KJU.Core.Input.StringLocation location = new KJU.Core.Input.StringLocation(10);
            Assert.AreEqual(10, location.Position);
        }
    }
}
