namespace KJU.Core.AST
{
    using System.Collections.Generic;
    using KJU.Core.Lexer;

    public interface INode
    {
        Range InputRange { get; }

        IEnumerable<Node> Children();
    }
}