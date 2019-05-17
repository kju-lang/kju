namespace KJU.Core.AST
{
    using System.Collections.Generic;
    using Lexer;

    public abstract class Node : INode
    {
        protected Node(Range inputRange)
        {
            this.InputRange = inputRange;
        }

        public Range InputRange { get; }

        public virtual IEnumerable<Node> Children()
        {
            return new List<Node>();
        }
    }
}