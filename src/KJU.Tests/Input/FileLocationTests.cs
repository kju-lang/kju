namespace KJU.Tests.Input
{
    using KJU.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileLocationTests
    {
        [TestMethod]
        public void TestToString()
        {
            KJU.Core.Input.ILocation location = new KJU.Core.Input.FileLocation("TestFile.kju", 34, 21);
            Assert.AreEqual("TestFile.kju:34:21", location.ToString());
        }

        [TestMethod]
        public void TestParams()
        {
            KJU.Core.Input.FileLocation location = new KJU.Core.Input.FileLocation("TestFile.kju", 34, 21);
            Assert.AreEqual("TestFile.kju", location.FileName);
            Assert.AreEqual(34, location.Line);
            Assert.AreEqual(21, location.Column);
        }
    }
}
