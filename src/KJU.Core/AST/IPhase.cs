namespace KJU.Core.AST
{
    using Diagnostics;

    public interface IPhase
    {
        void Run(Node root, IDiagnostics diagnostics);
    }
}