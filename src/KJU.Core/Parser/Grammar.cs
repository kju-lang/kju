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
        public IReadOnlyCollection<Rule<TLabel>> Rules { get; set; }

        public TLabel StartSymbol { get; set; }

        public CompiledGrammar<TLabel> CompileRegexes()
        {
            // TODO: IDfa<TLable, Rule>
            // note that Rule is nullable
            throw new NotImplementedException();
        }
    }
}
