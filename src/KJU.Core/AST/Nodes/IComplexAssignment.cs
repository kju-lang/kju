namespace KJU.Core.AST.Nodes
{
    public interface IComplexAssignment : INode
    {
        Expression Lhs { get; set; }

        Expression Value { get; set; }
    }
}