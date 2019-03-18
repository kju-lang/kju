namespace KJU.Tests.Input
{
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using KJU.Core.Input;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringInputReaderTests
    {
        [TestMethod]
        public void TestKeys()
        {
            var inputReader = new StringInputReader("test");
            var data = inputReader.Read();
            var expected = new List<int> { 0, 1, 2, 3, 4 };
            var actual = data.Select(kvp => ((StringLocation)kvp.Key).Position).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestValues()
        {
            var inputReader = new StringInputReader("test");
            var data = inputReader.Read();
            var expected = "test" + Constants.EndOfInput;
            var actual = string.Concat(data.Select(kvp => kvp.Value));
            Assert.AreEqual(expected, actual);
        }
    }
}