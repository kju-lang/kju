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

        public static IEnumerable<object[]> Executable { get; } = new KjuExamplesGetter().Examples
            .Where(example => !example.IsDisabled)
            .Where(example => example.IsPositive)
            .Where(example => example.Executable)
            .Select(example => new object[] { example });

        public static IEnumerable<object[]> NotExecutable { get; } = new KjuExamplesGetter().Examples
            .Where(example => !example.IsDisabled)
            .Where(example => example.IsPositive)
            .Where(example => !example.Executable)
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
        [Timeout(6000)]
        [DynamicData(nameof(Executable))]
        public void ExecutableExamples(IKjuExample example)
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
                var exitCode = process.ExitCode;
                if (!example.Ends)
                {
                    Assert.Fail($"Should not end but ended with exit code {exitCode}.");
                }

                Assert.AreEqual(
                    example.ExpectedExitCode,
                    exitCode,
                    $"Process returned with wrong exit code.");
            }

            var processOutput = process.StandardOutput.ReadToEnd();

            var outputCheckResult = example.OutputChecker.CheckOutput(processOutput);
            outputCheckResult.Notes.ForEach(Console.WriteLine);
            if (outputCheckResult is OutputCheckResult.Wrong)
            {
                Assert.Fail("Output is wrong.");
            }
        }

        [DataTestMethod]
        [Timeout(2000)]
        [DynamicData(nameof(NotExecutable))]
        public void NotExecutableExamples(IKjuExample example)
        {
            Assert.Inconclusive("unknown");
        }
    }
}