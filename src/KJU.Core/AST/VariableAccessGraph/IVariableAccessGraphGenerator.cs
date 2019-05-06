namespace KJU.Core.AST.VariableAccessGraph
{
    using System.Collections.Generic;
    using NodeVariableAccessMapping =
        System.Collections.Generic.IReadOnlyDictionary<Node,
            System.Collections.Generic.IReadOnlyCollection<VariableDeclaration>>;

    public interface IVariableAccessGraphGenerator
    {
        NodeVariableAccessMapping GetVariableInfoPerAstNode(
            Node root, VariableInfo variableInfo);
    }
}