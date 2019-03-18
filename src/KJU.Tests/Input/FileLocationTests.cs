namespace KJU.Tests.Input
{
    using KJU.Core.Input;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileLocationTests
    {
        [TestMethod]
        public void TestToString()
        {
            var location = new FileLocation("TestFile.kju", 34, 21);
            Assert.AreEqual("TestFile.kju:34:21", location.ToString());
        }

        [TestMethod]
        public void TestParams()
        {
            var location = new FileLocation("TestFile.kju", 34, 21);
            Assert.AreEqual("TestFile.kju", location.FileName);
            Assert.AreEqual(34, location.Line);
            Assert.AreEqual(21, location.Column);
        }
    }
}
