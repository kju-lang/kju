namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;

    public static class LexerUtils
    {
        public static IEnumerable<Token<TLabel>> ScanPreprocessed<TLabel>(
            this Lexer<TLabel> lexer,
            IEnumerable<KeyValuePair<ILocation, char>> input)
        {
            var preprocessor = new Preprocessor();
            var processedInput = preprocessor.PreprocessInput(input);
            return lexer.Scan(processedInput);
        }

        public static IEnumerable<Token<TLabel>> ScanPreprocessed<TLabel>(this Lexer<TLabel> lexer, string input)
        {
            return lexer.ScanPreprocessed(new StringInputReader(input).Read());
        }
    }
}