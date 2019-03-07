namespace KJU.Tests
{
    using System;
    using KJU.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Example tests class.
    /// </summary>
    [TestClass]
    public class HelloWorldTests
    {
        /// <summary>
        /// Verify whether example class returns good value.
        /// </summary>
        [TestMethod]
        public void TestHelloWorld()
        {
            KJU.Core.HelloWorld helloWorld = new KJU.Core.HelloWorld();
            Assert.AreEqual("Hello World!", helloWorld.Hello());
        }

        [TestMethod]
        public void TestHelloWorldMock()
        {
            Mock<Func<int, int>> mock = new Mock<Func<int, int>>();
            mock.Setup(m => m(0)).Returns(1);
            Assert.AreEqual(1, mock.Object(0));
        }
    }
}
