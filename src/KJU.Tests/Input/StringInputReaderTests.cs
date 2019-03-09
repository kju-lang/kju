namespace KJU.Tests.Input
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringInputReaderTests
    {
        [TestMethod]
        public void Simple()
        {
            KJU.Core.Input.IInputReader inputReader = new KJU.Core.Input.StringInputReader("test");
            List<KeyValuePair<KJU.Core.Input.ILocation, char>> data = inputReader.Read();
            Assert.AreEqual("test" + KJU.Core.Constants.EndOfInput, string.Concat(data.Select(kvp => kvp.Value)));
            CollectionAssert.AreEqual(new List<int>() { 0, 1, 2, 3, 4 }, data.Select(kvp => ((KJU.Core.Input.StringLocation)kvp.Key).Position).ToList());
        }
    }
}
