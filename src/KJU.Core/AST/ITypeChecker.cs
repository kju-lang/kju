namespace KJU.Core.AST
{
    using Diagnostics;

    public interface ITypeChecker
    {
        void LinkTypes(Node root, IDiagnostics diagnostics);
    }
}