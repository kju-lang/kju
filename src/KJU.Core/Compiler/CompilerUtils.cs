namespace KJU.Core.Compiler
{
    using Diagnostics;
    using Input;

    public static class CompilerUtils
    {
        public static Artifacts RunOnFile(this ICompiler compiler, string path, IDiagnostics diag)
        {
            var fileInputReader = new FileInputReader(path);
            return compiler.RunOnInputReader(fileInputReader, diag);
        }

        public static Artifacts RunOnString(this ICompiler compiler, string text, IDiagnostics diag)
        {
            var fileInputReader = new StringInputReader(text);
            return compiler.RunOnInputReader(fileInputReader, diag);
        }
    }
}
