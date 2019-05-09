namespace KJU.Tests.Examples
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using KJU.Core.Filenames;
    using Util;

    public class KjuExamplesGetter
    {
        private readonly string examplesDir;
        private readonly TestSpecValidator testSpecValidator = new TestSpecValidator();

        public KjuExamplesGetter(string examplesDir = null)
        {
            this.examplesDir = examplesDir ?? GetDefaultPath();
        }

        public IEnumerable<IKjuExample> Examples => Directory
            .GetFiles(this.examplesDir, "*.kju", SearchOption.AllDirectories)
            .Select(this.GetKjuExample);

        private static string GetDefaultPath()
        {
            return Path.GetFullPath($"{KjuHome.Path}/examples/kju");
        }

        private IKjuExample GetKjuExample(string path)
        {
            var specPath = path.ChangeExtension("spec.xml");
            var specs = File.Exists(specPath) ? XDocument.Load(specPath) : null;
            if (specs != null)
            {
                this.testSpecValidator.ValidateSpecs(specs, specPath);
            }

            return new KjuFileExample(path, specs);
        }
    }
}