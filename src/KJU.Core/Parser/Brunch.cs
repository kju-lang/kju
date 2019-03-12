namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Input;
    using KJU.Core.Lexer;

    // am am

    public class Brunch<TLabel> : ParseTree<TLabel>
    {
        public Rule<TLabel> Rule { get; set; }

        public IReadOnlyList<ParseTree<TLabel>> Children { get; set; }
    }
}
