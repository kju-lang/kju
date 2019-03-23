namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using static KJU.Core.Lexer.LexerUtils;

    public static class KjuParserUtils
    {
        public static ParseTree<KjuAlphabet> Parse(this Parser<KjuAlphabet> parser, List<KeyValuePair<ILocation, char>> input, IDiagnostics diag)
        {
            var tokens = KjuLexerFactory.Instance.ScanPreprocessed(input, diag).Where(token => token.Category != KjuAlphabet.Whitespace);
            return parser.Parse(tokens, diag);
        }

        public static ParseTree<KjuAlphabet> Parse(this Parser<KjuAlphabet> parser, string input, IDiagnostics diag)
        {
            var tokens = KjuLexerFactory.Instance.ScanPreprocessed(input, diag).Where(token => token.Category != KjuAlphabet.Whitespace);
            return parser.Parse(tokens, diag);
        }
    }
}
