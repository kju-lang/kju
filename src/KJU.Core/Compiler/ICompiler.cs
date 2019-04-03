namespace KJU.Core.Compiler
{
    using Diagnostics;
    using KJU.Core.Input;

    public interface ICompiler
    {
        void RunOnInputReader(IInputReader inputReader, IDiagnostics diagnostics);
    }
}