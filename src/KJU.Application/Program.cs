namespace KJU.Application
{
    using System;
    using System.IO;
    using KJU.Core;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Parser;

    public class Program
    {
        public static void Main(string[] args)
        {
            foreach (string filename in args)
            {
                IDiagnostics diag = new TextWriterDiagnostics(Console.Error);

                try
                {
                    string data = File.ReadAllText(filename);
                    Console.WriteLine($"compiling {data}...");
                    var tree = KjuParserFactory.Instance.Parse(data, diag);
                    Console.WriteLine($"tree: {tree}");
                }
                catch (Exception ex) when (
                       ex is ParseException
                    || ex is FormatException
                    || ex is PreprocessorException)
                {
                    diag.Report();
                }
            }
        }
    }
}
