namespace KJU.Core.AST
{
    using Diagnostics;

    public interface INameResolver
    {
        void LinkNames(Node root, IDiagnostics diagnostics);
    }
}