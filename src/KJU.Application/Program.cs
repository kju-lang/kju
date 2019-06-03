namespace KJU.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CommandLine;
    using KJU.Core;
    using KJU.Core.AST;
    using KJU.Core.Compiler;
    using KJU.Core.Diagnostics;
    using KJU.Core.Filenames;
    using KJU.Core.Input;
    using KJU.Core.Parser;
    using static KJU.Application.ProgramUtils;

    public static class Program
    {
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(options => Run(options));
        }

        public static void Run(Options options)
        {
            var compiler = new Compiler();
            var inputs = options.Files.Select(file => new CompilationQuery(new FileInputReader(file), file.RemoveExtension()));
            foreach (var query in inputs)
            {
                var diag = new TextWriterDiagnostics(Console.Error);

                try
                {
                    GenerateArtifacts(options, compiler, query, diag);
                }
                catch (CompilerException ex)
                {
                    Console.WriteLine("Compilation failed");
                    throw ex;
                }
                catch (ArtifactGenerationException e)
                {
                    Console.WriteLine($"Cannot generate artifact: {e.Message}");
                }
                finally
                {
                    diag.Report();
                }
            }
        }

        public static void GenerateArtifacts(
            Options options,
            ICompiler compiler,
            CompilationQuery query,
            IDiagnostics diag)
        {
            var artifacts = compiler.RunOnInputReader(query.Input, diag);

            var resultPath = query.ResultPath;
            if (options.GenAstDot)
            {
                var astPath = resultPath.AddExtension("ast.dot");
                var astText = Core.Visualization.AstToDotConverter.Convert(artifacts.Ast);
                File.WriteAllLines(astPath, astText);
            }

            if (options.GenAsm || options.GenExe)
            {
                var asmPath = resultPath.AddExtension("asm");
                File.WriteAllLines(asmPath, artifacts.Asm);
                if (options.GenExe)
                {
                    var oPath = resultPath.AddExtension("o");
                    var stdlibPath = resultPath.AddExtension("stdlib.o");
                    File.WriteAllBytes(stdlibPath, BundledStdlib.data);
                    var arguments = $"{asmPath} -f elf64 -o {oPath}";
                    var nasmExitCode = RunProcess("nasm", arguments);
                    if (nasmExitCode != 0)
                    {
                        throw new ArtifactGenerationException($"Nasm {arguments} process failed. Exit code: {nasmExitCode}");
                    }

                    var exePath = resultPath;

                    var gccExitCode = RunProcess(@"g++", $"-std=c++14 -no-pie {oPath} {stdlibPath} -o {exePath}");

                    if (gccExitCode != 0)
                    {
                        throw new ArtifactGenerationException(
                            $"Gcc (linking) process failed. Exit code: {gccExitCode}");
                    }
                }
            }
        }

        public class Options
        {
            [Value(0, MetaName = "FILES", HelpText = "Source files.")]
            public IEnumerable<string> Files { get; set; }

            [Option("gen-ast-dot", Default = false, HelpText = "Generate AST dot graph.")]
            public bool GenAstDot { get; set; }

            [Option("gen-asm", Default = false, HelpText = "Generate asm output.")]
            public bool GenAsm { get; set; }

            [Option("gen-exe", Default = false, HelpText = "Generate executable.")]
            public bool GenExe { get; set; }
        }
    }
}
