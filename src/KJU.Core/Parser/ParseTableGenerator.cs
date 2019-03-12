namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;

    public class ParseTableGenerator<TLabel>
    {
        public static IReadOnlyDictionary<Tuple<DfaState<TLabel>, TLabel>, ParseAction<TLabel>> Parse(CompiledGrammar<TLabel> grammar)
        {
            throw new NotImplementedException();
        }
    }
}