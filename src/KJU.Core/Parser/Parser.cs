namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;

    public class Parser<TLabel>
    {
        public Parser(CompiledGrammar<TLabel> grammar, IReadOnlyDictionary<DfaState<TLabel>, ParseAction<TLabel>> table)
        {
        }

        public ParseTree<TLabel> Parse(IEnumerable<TLabel> tokens)
        {
            throw new NotImplementedException();
        }
    }
}