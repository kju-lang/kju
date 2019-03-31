namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using KJU.Tests.Util;

    public class KjuExamplesGetter
    {
        private readonly string examplesDir;

        public KjuExamplesGetter(string examplesDir = null)
        {
            this.examplesDir = examplesDir ?? GetDefaultPath();
        }

        public IEnumerable<KjuExample> Examples => Directory
            .GetFiles(this.examplesDir, "*.kju", SearchOption.AllDirectories)
            .Select(path => new KjuExample(path));

        private static string GetDefaultPath()
        {
            var kjuHome = KjuHome.Path;
            return Path.GetFullPath($"{kjuHome}/examples/kju");
        }
    }
}