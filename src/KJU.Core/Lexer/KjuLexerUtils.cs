namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;

    public static class KjuLexerUtils
    {
        public static IEnumerable<Token<KjuAlphabet>> ScanPreprocessed(this Lexer<KjuAlphabet> lexer, List<KeyValuePair<ILocation, char>> input)
        {
            var preprocessor = new Preprocessor();
            var processedInput = preprocessor.PreprocessInput(input);
            return lexer.Scan(processedInput).Where(token => token.Category != KjuAlphabet.Whitespace);
        }

        public static IEnumerable<Token<KjuAlphabet>> ScanPreprocessed(this Lexer<KjuAlphabet> lexer, string input)
        {
            return lexer.ScanPreprocessed(new StringInputReader(input).Read());
        }
    }
}