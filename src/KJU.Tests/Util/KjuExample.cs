namespace KJU.Tests.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;

    public class KjuExample
    {
        private const string DefaultSpecText = @"
<Spec>
    <IsPositive>true</IsPositive>
    <IsDisabled>false</IsDisabled>
    <ExpectedMagicStrings>
    </ExpectedMagicStrings>
</Spec>
";

        private string specPath;

        private XDocument spec;

        private XDocument defaultSpec;

        public KjuExample(string path)
        {
            this.Path = path;

            string fileName = new FileInfo(this.Path).Name;
            if (Regex.Matches(fileName, @"\.[^.]*$").Count == 0)
            {
                throw new ArgumentException($"KJU example path has no extension: {this.Path}");
            }

            this.specPath = Regex.Replace(this.Path, @"(\.[^.]*)$", ".spec.xml");
            if (File.Exists(this.specPath))
            {
                this.spec = XDocument.Load(this.specPath);
            }

            this.defaultSpec = XDocument.Parse(DefaultSpecText);
        }

        public string Name => this.GetDefaultName();

        public string Path { get; }

        public bool IsPositive => bool.Parse(this.GetProperty("/Spec/IsPositive"));

        public bool IsDisabled => bool.Parse(this.GetProperty("/Spec/IsDisabled"));

        public IEnumerable<string> ExpectedMagicStrings => this.GetPropertyList("/Spec/ExpectedMagicStrings", "MagicString");

        public override string ToString()
        {
            return $"{this.Name} ({this.Path})";
        }

        private static string GetProperty(string xpath, XNode spec)
        {
            return ((IEnumerable)spec?.XPathEvaluate(xpath))
                ?.Cast<XElement>()
                .Select((x) => x.Value)
                .FirstOrDefault();
        }

        private static IEnumerable<string> GetPropertyList(string xpath, string field, XNode spec)
        {
            if (GetProperty(xpath, spec) != null)
            {
                return ((IEnumerable)spec?.XPathEvaluate($"{xpath}/{field}"))
                    ?.Cast<XElement>()
                    .Select((x) => x.Value);
            }
            else
            {
                return null;
            }
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
            var result = fileName;
            result = Regex.Replace(result, @"(\.[^.]*)$", string.Empty);
            result = Regex.Replace(result, @"_", " ");
            result = char.ToUpper(result[0]) + result.Substring(1);
            return result;
        }
    }
}
