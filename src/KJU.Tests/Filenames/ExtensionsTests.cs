namespace KJU.Tests.Filenames
{
    using KJU.Core.Filenames;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void TestExtensionRemover()
        {
            Assert.AreEqual("example", Extensions.RemoveExtension("example"));
            Assert.AreEqual("example", Extensions.RemoveExtension("example.kju"));
            Assert.AreEqual("example", Extensions.RemoveExtension("example.ast.dot"));
            Assert.AreEqual("example", Extensions.RemoveExtension("example.spec.xml"));
            Assert.AreEqual("example", Extensions.RemoveExtension("example.cs"));
            Assert.AreEqual("example", Extensions.RemoveExtension("example.o"));
        }

        [TestMethod]
        public void TestExtensionChanger()
        {
            Assert.AreEqual("example.ast.dot", Extensions.ChangeExtension("example", "ast.dot"));
            Assert.AreEqual("example.ast.dot", Extensions.ChangeExtension("example.kju", "ast.dot"));
            Assert.AreEqual("example.kju", Extensions.ChangeExtension("example.ast.dot", "kju"));
        }
    }
}
