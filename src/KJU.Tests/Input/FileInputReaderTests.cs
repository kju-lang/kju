namespace KJU.Tests.Input
{
    using System.Linq;
    using KJU.Core.Input;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileInputReaderTests
    {
        [TestMethod]
        public void Simple()
        {
            var examplePath = "../../../Input/example.kju";
            var inputReader = new FileInputReader(examplePath);
            var data = inputReader.Read();
            Assert.AreEqual("fun kju():Unit{\n	var x:Int={\n		5;\n	}\n}\n" + KJU.Core.Constants.EndOfInput, string.Concat(data.Select(kvp => kvp.Value)));

            Assert.AreEqual(1, ((KJU.Core.Input.FileLocation)data[0].Key).Line);
            Assert.AreEqual(1, ((KJU.Core.Input.FileLocation)data[0].Key).Column);
            Assert.AreEqual(examplePath, ((KJU.Core.Input.FileLocation)data[0].Key).FileName);

            Assert.AreEqual(1, ((KJU.Core.Input.FileLocation)data[1].Key).Line);
            Assert.AreEqual(2, ((KJU.Core.Input.FileLocation)data[1].Key).Column);
            Assert.AreEqual(examplePath, ((KJU.Core.Input.FileLocation)data[1].Key).FileName);
        }
    }
}
