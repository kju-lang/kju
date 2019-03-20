namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;
    using KJU.Core.Lexer;

    public static class KjuParser
    {
        public static readonly Parser<KjuAlphabet> Instance = ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);

        public static ParseTree<KjuAlphabet> Parse(IEnumerable<Token<KjuAlphabet>> tokens)
        {
            var tokensAndEof = new List<Token<KjuAlphabet>>();
            tokensAndEof.AddRange(tokens);
            tokensAndEof.Add(new Token<KjuAlphabet> { Category = KjuAlphabet.Eof });
            return Instance.Parse(tokensAndEof);
        }

        public static ParseTree<KjuAlphabet> Parse(List<KeyValuePair<ILocation, char>> input)
        {
            return Parse(KjuLexer.Scan(input));
        }

        public static ParseTree<KjuAlphabet> Parse(string input)
        {
            return Parse(KjuLexer.Scan(input));
        }
    }
}