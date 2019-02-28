using Microsoft.VisualStudio.TestTools.UnitTesting;
using KJU.Core;

namespace KJU.Tests
{
    [TestClass]
    public class HelloWorldTests
    {
        [TestMethod]
        public void TestHelloWorld()
        {
            KJU.Core.HelloWorld helloWorld = new KJU.Core.HelloWorld();
            Assert.AreEqual("Hello World!", helloWorld.hello());
        }
    }
}
