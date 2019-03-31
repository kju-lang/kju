namespace KJU.Application
{
    using System;
    using System.IO;
    using KJU.Core;
    using KJU.Core.AST;
    using KJU.Core.Compiler;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Parser;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var compiler = new Compiler();
            foreach (var filename in args)
            {
                var diag = new TextWriterDiagnostics(Console.Error);

                try
                {
                    compiler.Run(filename, diag);
                }
                catch (CompilerException)
                {
                }
                finally
                {
                    diag.Report();
                }
            }
        }
    }
}