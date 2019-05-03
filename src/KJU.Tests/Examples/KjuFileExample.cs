namespace KJU.Tests.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using KJU.Core.Filenames;
    using KJU.Core.Input;
    using OutputChecker;
    using static System.Text.RegularExpressions.Regex;

    public class KjuFileExample : IKjuExample
    {
        private const string DefaultSpecText = @"
<Spec>
    <IsPositive>true</IsPositive>
    <IsDisabled>false</IsDisabled>
    <ExpectedMagicStrings>
    </ExpectedMagicStrings>
    <Execution>
        <Executable>true</Executable>
        <Timeout>10000</Timeout>
        <Input></Input>
        <ExpectedOutput></ExpectedOutput>
        <NormalizeOutput>true</NormalizeOutput>
        <Ends>true</Ends>
    </Execution>
</Spec>
";

        private readonly XDocument spec;

        private readonly XDocument defaultSpec;

        public KjuFileExample(string path)
        {
            this.Path = path;

            var specPath = this.Path.ChangeExtension("spec.xml");
            if (File.Exists(specPath))
            {
                this.spec = XDocument.Load(specPath);
            }

            this.defaultSpec = XDocument.Parse(DefaultSpecText);
        }

        public IInputReader Program => new FileInputReader(this.Path);

        public string SimpleName => new FileInfo(this.Path).Name.RemoveExtension();

        public string Name => this.GetDefaultName();

        public string Path { get; }

        public bool IsPositive => bool.Parse(this.GetProperty("/Spec/IsPositive"));

        public bool IsDisabled => bool.Parse(this.GetProperty("/Spec/IsDisabled"));

        public bool Executable => bool.Parse(this.GetProperty("/Spec/Execution/Executable"));

        public string Input => this.GetProperty("/Spec/Execution/Input");

        public bool Ends => bool.Parse(this.GetProperty("/Spec/Execution/Ends"));

        public int Timeout => int.Parse(this.GetProperty("/Spec/Execution/Timeout"));

        public IOutputChecker OutputChecker =>
            this.ExpectedOutput != null
                ? (IOutputChecker)new ExactOutputChecker(this.ExpectedOutput)
                : new AcceptAllChecker();

        public IEnumerable<string> ExpectedMagicStrings =>
            this.GetPropertyList("/Spec/ExpectedMagicStrings", "MagicString");

        private string ExpectedOutput => this.GetProperty("/Spec/Execution/ExpectedOutput");

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
            var withoutUnderscore = Replace(this.SimpleName, @"_", " ");
            return char.ToUpper(withoutUnderscore[0]) + withoutUnderscore.Substring(1);
        }
    }
}