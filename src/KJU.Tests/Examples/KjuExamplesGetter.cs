namespace KJU.Tests.Examples
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Util;

    public class KjuExamplesGetter
    {
        private readonly string examplesDir;

        public KjuExamplesGetter(string examplesDir = null)
        {
            this.examplesDir = examplesDir ?? GetDefaultPath();
        }

        public IEnumerable<IKjuExample> Examples => Directory
            .GetFiles(this.examplesDir, "*.kju", SearchOption.AllDirectories)
            .Select(path => new KjuFileExample(path));

        private static string GetDefaultPath()
        {
            var kjuHome = KjuHome.Path;
            return Path.GetFullPath($"{kjuHome}/examples/kju");
        }
    }
}