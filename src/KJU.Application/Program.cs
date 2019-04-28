namespace KJU.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CommandLine;
    using KJU.Core;
    using KJU.Core.AST;
    using KJU.Core.Compiler;
    using KJU.Core.Diagnostics;
    using KJU.Core.Filenames;
    using KJU.Core.Input;
    using KJU.Core.Parser;

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
            foreach (var filename in options.Files)
            {
                var diag = new TextWriterDiagnostics(Console.Error);

                try
                {
                    var artifacts = compiler.RunOnFile(filename, diag);

                    if (options.GenAstDot)
                    {
                        System.IO.File.WriteAllLines(
                            Extensions.ChangeExtension(filename, "ast.dot"),
                            KJU.Core.Visualization.AstToDotConverter.Convert(artifacts.Ast));
                    }

                    if (options.GenAsm)
                    {
                    Console.WriteLine($"{Extensions.ChangeExtension(filename, "asm")}");
                        System.IO.File.WriteAllLines(
                            Extensions.ChangeExtension(filename, "asm"),
                            artifacts.Asm);
                    }
                }
                catch (CompilerException)
                {
                    Console.WriteLine("Compilation failed");
                }
                finally
                {
                    diag.Report();
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
        }
    }
}
