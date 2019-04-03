namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;

    public class CallGraphGenerator
    {
        public IReadOnlyDictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>> BuildCallGraph(Node root)
        {
            throw new NotImplementedException();
        }
    }
}