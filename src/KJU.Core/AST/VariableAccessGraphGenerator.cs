namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;

    public class VariableAccessGraphGenerator
    {
        public IReadOnlyDictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>> BuildVariableAccessGraph(Node root)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>> BuildVariableModificationGraph(Node root)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<Node, IReadOnlyCollection<VariableDeclaration>> BuildVariableModificationsPerAstNode(Node root, IReadOnlyDictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>> modificationGraph)
        {
            throw new NotImplementedException();
        }
    }
}