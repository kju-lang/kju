namespace KJU.Core.AST
{
    using System.Collections.Generic;
    using System.Linq;
    using Lexer;

    public abstract class Node
    {
        public Range InputRange { get; set; }

        public virtual IEnumerable<Node> Children()
        {
            return new List<Node>();
        }

        public virtual string Representation()
        {
            return $"{this.GetType().Name}:{{{string.Join(", ", this.Children().Select(x => x.Representation()))}}}";
        }
    }
}