namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;
    using KJU.Core.Util;

    public class ParserFactory<TLabel>
    where TLabel : Enum
    {
        public static Parser<TLabel> MakeParser(Grammar<TLabel> grammar, TLabel eofSymbol)
        {
            var compiledGrammar = GrammarCompiler<TLabel>.CompileGrammar(grammar);
            var nullables = NullablesHelper<TLabel>.GetNullableSymbols(compiledGrammar);
            var first = FirstHelper<TLabel>.GetFirstSymbols(compiledGrammar, nullables);
            var firstInversed = first.InverseRelation();
            var follow = FollowHelper<TLabel>.GetFollowSymbols(compiledGrammar, nullables, firstInversed, eofSymbol);
            var followInversed = follow.InverseRelation();
            var firstPlus = FirstPlusHelper<TLabel>.GetFirstPlusSymbols(firstInversed, followInversed, nullables);
            var table = ParseTableGenerator<TLabel>.Parse(compiledGrammar, followInversed, firstPlus);
            return new Parser<TLabel>(compiledGrammar, table);
        }
    }
}