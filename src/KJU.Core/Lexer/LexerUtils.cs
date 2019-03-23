namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;

    public static class LexerUtils
    {
        public static IEnumerable<Token<TLabel>> ScanPreprocessed<TLabel>(
            this Lexer<TLabel> lexer,
            IEnumerable<KeyValuePair<ILocation, char>> input,
            IDiagnostics diag)
        {
            var preprocessor = new Preprocessor();
            var processedInput = preprocessor.PreprocessInput(input, diag);
            return lexer.Scan(processedInput, diag);
        }

        public static IEnumerable<Token<TLabel>> ScanPreprocessed<TLabel>(this Lexer<TLabel> lexer, string input, IDiagnostics diag)
        {
            return lexer.ScanPreprocessed(new StringInputReader(input).Read(), diag);
        }
    }
}
