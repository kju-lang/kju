namespace KJU.Tests.Examples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application;
    using Core.Compiler;
    using Core.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OutputChecker;

    [TestClass]
    public class ExamplesRunTests
    {
        public const string TestsDirectory = "KjuRunTests";

        public static readonly ICompiler Compiler = new Compiler();

        public static IEnumerable<object[]> Positive { get; } = new KjuExamplesGetter().Examples
            .Where(example => !example.IsDisabled)
            .Where(example => example.IsPositive)
            .Where(example => example.Executable)
            .Select(example => new object[] { example });

        public static IEnumerable<object[]> PositiveDisabled { get; } = new KjuExamplesGetter().Examples
            .Where(example => example.IsDisabled)
            .Where(example => example.IsPositive)
            .Where(example => example.Executable)
            .Select(example => new object[] { example });

        [ClassInitialize]
        public static void TestInit(TestContext ignored)
        {
            if (Directory.Exists(TestsDirectory))
            {
                Directory.Delete(TestsDirectory, true);
            }

            Directory.CreateDirectory(TestsDirectory);
        }

        [DataTestMethod]
        [Timeout(2000)]
        [DynamicData(nameof(Positive))]
        public void PositiveExamples(IKjuExample example)
        {
            var options = new Program.Options
            {
                GenExe = true
            };
            var exeName = $"{TestsDirectory}/{example.SimpleName}";
            var query = new CompilationQuery(example.Program, exeName);
            var doNothingDiagnostics = new DoNothingDiagnostics();
            Program.GenerateArtifacts(options, Compiler, query, doNothingDiagnostics);
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = exeName,
                    Arguments = string.Empty,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            process.Start();
            using (var writer = process.StandardInput)
            {
                writer.Write(example.Input);
            }

            if (!process.WaitForExit(example.Timeout))
            {
                process.Kill();
                if (example.Ends)
                {
                    Assert.Fail($"Process has not ended before timeout ({example.Timeout}).");
                }
            }
            else
            {
                if (!example.Ends)
                {
                    Assert.Fail($"Should not end but ended.");
                }
            }

            var processOutput = process.StandardOutput.ReadToEnd();
            var exitCode = process.ExitCode;
            Assert.AreEqual(0, exitCode, $"Process returned with non zero code: {exitCode}.");

            var outputCheckResult = example.OutputChecker.CheckOutput(processOutput);
            outputCheckResult.Notes.ForEach(Console.WriteLine);
            if (outputCheckResult is OutputCheckResult.Wrong)
            {
                Assert.Fail("Output is wrong.");
            }
        }

        [DataTestMethod]
        [Timeout(2000)]
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
    }
}