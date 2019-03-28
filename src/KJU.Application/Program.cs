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

    public class Program
    {
        public static void Main(string[] args)
        {
            var compiler = new Compiler();
            foreach (string filename in args)
            {
                IDiagnostics diag = new TextWriterDiagnostics(Console.Error);

                try
                {
                    compiler.Run(filename, diag);
                }
                catch (Exception ex) when (
                       ex is ParseException
                    || ex is FormatException
                    || ex is PreprocessorException)
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
