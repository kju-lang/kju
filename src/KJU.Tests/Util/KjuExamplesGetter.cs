namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using KJU.Tests.Util;

    public class KjuExamplesGetter
    {
        private string examplesDir;

        public KjuExamplesGetter(string examplesDir = null)
        {
            if (examplesDir != null) {
                this.examplesDir = examplesDir;
            } else {
                string kjuHome = KjuHome.Path;
                this.examplesDir = Path.GetFullPath($"{kjuHome}/examples/kju");
            }
        }

        public IEnumerable<KjuExample> Examples => Directory
            .GetFiles(this.examplesDir, "*.kju", SearchOption.AllDirectories)
            .Select(path => new KjuExample(path));
    }
}
