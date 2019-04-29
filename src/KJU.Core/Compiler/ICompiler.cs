namespace KJU.Core.Compiler
{
    using Diagnostics;
    using KJU.Core.Input;

    public interface ICompiler
    {
        Artifacts RunOnInputReader(IInputReader inputReader, IDiagnostics diagnostics);
    }
}
