namespace KJU.Core.AST.Nodes
{
    public interface IArrayAssignment
    {
        Expression Lhs { get; set; }

        Expression Value { get; set; }
    }
}