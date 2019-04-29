namespace KJU.Tests.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using KJU.Core.Compiler;
    using KJU.Core.Diagnostics;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PositiveExamplesTests
    {
        public static readonly ICompiler Compiler = new Compiler();

        public static IEnumerable<object[]> Data
        {
            get
            {
                var examplesGetter = new KjuExamplesGetter();
                return examplesGetter.Examples
                    .Where(example => !example.IsDisabled)
                    .Where(example => example.IsPositive)
                    .Select(example => new object[] { example });
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(Data), DynamicDataSourceType.Property)]
        public void TestExamplesSpecification(KjuExample example)
        {
            var diag = new Mock<IDiagnostics>();
            Compiler.RunOnFile(example.Path, diag.Object);
            MockDiagnostics.Verify(diag, example.ExpectedMagicStrings.ToArray());
        }
    }
}
