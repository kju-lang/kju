namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using static KJU.Core.Lexer.LexerUtils;

    public static class KjuParserFactory
    {
        public static readonly Parser<KjuAlphabet> Instance = ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);
    }
}