namespace KJU.Tests.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using static KJU.Core.Filenames.Extensions;
    using static System.Text.RegularExpressions.Regex;

    public class KjuExample
    {
        private const string DefaultSpecText = @"
<Spec>
    <IsPositive>true</IsPositive>
    <IsDisabled>false</IsDisabled>
    <ExpectedMagicStrings>
    </ExpectedMagicStrings>
    <Execution>
        <Input></Input>
        <ExpectedOutput></ExpectedOutput>
        <Ends>true</Ends>
    </Execution>
</Spec>
";

        private readonly XDocument spec;

        private readonly XDocument defaultSpec;

        public KjuExample(string path)
        {
            this.Path = path;

            var specPath = ChangeExtension(this.Path, "spec.xml");
            if (File.Exists(specPath))
            {
                this.spec = XDocument.Load(specPath);
            }

            this.defaultSpec = XDocument.Parse(DefaultSpecText);
        }

        public string Name => this.GetDefaultName();

        public string Path { get; }

        public bool IsPositive => bool.Parse(this.GetProperty("/Spec/IsPositive"));

        public bool IsDisabled => bool.Parse(this.GetProperty("/Spec/IsDisabled"));

        public string Input => this.GetProperty("/Spec/Execution/Input");

        public bool Ends => bool.Parse(this.GetProperty("/Spec/Execution/Ends"));

        public string ExpectedOutput => this.GetProperty("/Spec/Execution/ExpectedOutput");

        public IEnumerable<string> ExpectedMagicStrings =>
            this.GetPropertyList("/Spec/ExpectedMagicStrings", "MagicString");

        public override string ToString()
        {
            return $"{this.Name} ({this.Path})";
        }

        private static string GetProperty(string xpath, XNode spec)
        {
            var evaluation = (IEnumerable)spec?.XPathEvaluate(xpath);
            return evaluation?
                .Cast<XElement>()
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        private static IEnumerable<string> GetPropertyList(string xpath, string field, XNode spec)
        {
            if (GetProperty(xpath, spec) != null)
            {
                return ((IEnumerable)spec?.XPathEvaluate($"{xpath}/{field}"))
                    ?.Cast<XElement>()
                    .Select(x => x.Value);
            }

            return null;
        }

        private string GetProperty(string xpath)
        {
            return GetProperty(xpath, this.spec) ?? GetProperty(xpath, this.defaultSpec);
        }

        private IEnumerable<string> GetPropertyList(string xpath, string field)
        {
            return GetPropertyList(xpath, field, this.spec) ?? GetPropertyList(xpath, field, this.defaultSpec);
        }

        private string GetDefaultName()
        {
            var fileName = new FileInfo(this.Path).Name;
            var withoutExtension = RemoveExtension(fileName);
            var withoutUnderscore = Replace(withoutExtension, @"_", " ");
            return char.ToUpper(withoutUnderscore[0]) + withoutUnderscore.Substring(1);
        }
    }
}