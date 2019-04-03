namespace KJU.Core.AST
{
    using System.Collections.Generic;
    using Lexer;

    public abstract class Node
    {
        public Range InputRange { get; set; }

        public virtual IEnumerable<Node> Children()
        {
            return new List<Node>();
        }
    }
}