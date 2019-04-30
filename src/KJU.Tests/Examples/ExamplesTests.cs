namespace KJU.Tests.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Compiler;
    using Core.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;

    [TestClass]
    public class ExamplesTests
    {
        public static readonly ICompiler Compiler = new Compiler();

        public static IEnumerable<object[]> Positive { get; } = new KjuExamplesGetter().Examples
            .Where(example => !example.IsDisabled)
            .Where(example => example.IsPositive)
            .Select(example => new object[] { example });

        public static IEnumerable<object[]> PositiveDisabled { get; } = new KjuExamplesGetter().Examples
            .Where(example => example.IsDisabled)
            .Where(example => example.IsPositive)
            .Select(example => new object[] { example });

        public static IEnumerable<object[]> Negative { get; } = new KjuExamplesGetter().Examples
            .Where(example => !example.IsDisabled)
            .Where(example => !example.IsPositive)
            .Select(example => new object[] { example });

        public static IEnumerable<object[]> NegativeDisabled { get; } = new KjuExamplesGetter().Examples
            .Where(example => example.IsDisabled)
            .Where(example => !example.IsPositive)
            .Select(example => new object[] { example });

        [DataTestMethod]
        [DynamicData(nameof(Positive))]
        public void PositiveExamples(IKjuExample example)
        {
            var diag = new Mock<IDiagnostics>();
            Compiler.RunOnInputReader(example.Program, diag.Object);
            MockDiagnostics.Verify(diag, example.ExpectedMagicStrings.ToArray());
        }

        [DataTestMethod]
        [DynamicData(nameof(Negative))]
        public void NegativeExamples(IKjuExample example)
        {
            var diag = new Mock<IDiagnostics>();

            Assert.ThrowsException<CompilerException>(() => Compiler.RunOnInputReader(example.Program, diag.Object));

            MockDiagnostics.Verify(diag, example.ExpectedMagicStrings.ToArray());
        }

        [DataTestMethod]
        [DynamicData(nameof(PositiveDisabled))]
        public void PositiveDisabledExamples(IKjuExample example)
        {
            try
            {
                this.PositiveExamples(example);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"{ex}");
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(NegativeDisabled))]
        public void NegativeDisabledExamples(IKjuExample example)
        {
            try
            {
                this.NegativeExamples(example);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"{ex}");
            }
        }
    }
}