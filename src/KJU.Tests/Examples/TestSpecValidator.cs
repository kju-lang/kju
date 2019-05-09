namespace KJU.Tests.Examples
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using KJU.Tests.Util;

    public class TestSpecValidator
    {
        public void ValidateSpecs(XDocument specs, string path)
        {
            var schemas = new XmlSchemaSet();
            var specsXsdPath = Path.GetFullPath($"{KjuHome.Path}/examples/spec.xsd");
            var fileReader = File.OpenRead(specsXsdPath);
            var xmlReader = XmlReader.Create(fileReader);
            schemas.Add(string.Empty, xmlReader);
            specs.Validate(
                schemas,
                (_, error) => throw new XmlSchemaValidationException($"File: {path}\n\n {error.Message}"));
        }
    }
}