namespace KJU.Core.AST.VariableAccessGraph
{
    using NodeVariableAccessMapping =
        System.Collections.Generic.IReadOnlyDictionary<Node,
            System.Collections.Generic.IReadOnlyCollection<VariableDeclaration>>;

    public class VariableAccess
    {
        public VariableAccess(NodeVariableAccessMapping accesses, NodeVariableAccessMapping modifies)
        {
            this.Accesses = accesses;
            this.Modifies = modifies;
        }

        public NodeVariableAccessMapping Accesses { get; }

        public NodeVariableAccessMapping Modifies { get; }
    }
}