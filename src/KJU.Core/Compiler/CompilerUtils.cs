namespace KJU.Core.Compiler
{
    using Diagnostics;
    using Input;

    public static class CompilerUtils
    {
        public static void RunOnFile(this ICompiler compiler, string path, IDiagnostics diag)
        {
            var fileInputReader = new FileInputReader(path);
            compiler.RunOnInputReader(fileInputReader, diag);
        }

        public static void RunOnString(this ICompiler compiler, string text, IDiagnostics diag)
        {
            var fileInputReader = new StringInputReader(text);
            compiler.RunOnInputReader(fileInputReader, diag);
        }
    }
}