namespace KJU.Core.AST.CallGraph
{
    using System.Collections.Generic;

    public interface ICallGraphGenerator
    {
        IReadOnlyDictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>> BuildCallGraph(Node root);
    }
}