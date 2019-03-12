namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;

    public class Grammar<TLabel>
    {
        public TLabel StartSymbol { get; set; }

        public IReadOnlyCollection<Rule<TLabel>> Rules { get; set; }
    }
}