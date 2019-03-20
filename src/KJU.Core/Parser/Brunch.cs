namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Input;
    using KJU.Core.Lexer;

    // om nom nom

    public class Brunch<TLabel> : ParseTree<TLabel>
    {
        public Rule<TLabel> Rule { get; set; }

        public IReadOnlyList<ParseTree<TLabel>> Children { get; set; }

        public override string ToString()
        {
            return $"{this.Rule.Lhs} [{string.Join(", ", this.Children)}]";
        }
    }
}
