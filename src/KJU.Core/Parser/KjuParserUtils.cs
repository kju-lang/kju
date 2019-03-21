namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using static KJU.Core.Lexer.KjuLexerUtils;

    public static class KjuParserUtils
    {
        public static ParseTree<KjuAlphabet> Parse(this Parser<KjuAlphabet> parser, List<KeyValuePair<ILocation, char>> input)
        {
            return parser.Parse(KjuLexerFactory.Instance.ScanPreprocessed(input));
        }

        public static ParseTree<KjuAlphabet> Parse(this Parser<KjuAlphabet> parser, string input)
        {
            return parser.Parse(KjuLexerFactory.Instance.ScanPreprocessed(input));
        }
    }
}