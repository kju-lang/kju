namespace KJU.Core.AST.Nodes
{
    public interface IContainerAccess : INode
    {
        Expression Lhs { get; }

        Expression Offset { get; }
    }
}
