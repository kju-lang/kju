namespace KJU.Core.AST.VariableAccessGraph
{
    using NodeVariableAccessMapping =
        System.Collections.Generic.IReadOnlyDictionary<Node,
            System.Collections.Generic.IReadOnlyCollection<VariableDeclaration>>;

    public interface IVariableAccessGraphGenerator
    {
        VariableAccess GetVariableInfoPerAstNode(Node root);
    }
}