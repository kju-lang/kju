namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;
    using KJU.Core.Util;

    public class CompiledGrammar<TLabel>
    {
        public IReadOnlyDictionary<TLabel, IDfa<Optional<Rule<TLabel>>, TLabel>> Rules { get; set; }

        public TLabel StartSymbol { get; set; }
    }
}