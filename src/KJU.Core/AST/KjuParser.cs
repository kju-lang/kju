namespace KJU.Core.AST
{
    using System.Collections.Generic;
    using Diagnostics;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;

    public static class KjuParser
    {
        public static ParseTree<KjuAlphabet> GenerateParseTree(string sourceCode)
        {
            var tokens = new List<Token<KjuAlphabet>>();
            var parser = ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);
            return parser.Parse(tokens);
        }
    }
}