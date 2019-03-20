namespace KJU.Core.Compiler
{
    using Diagnostics;

    public interface ICompiler
    {
        void Run(string path, IDiagnostics diagnostics);
    }
}