namespace KJU.Core.Input
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Diagnostics;

    public class Preprocessor
    {
        public const string UnexpectedCommentEndDiagnosticType = "Preprocessor.UnexpectedCommentEndToken";
        public const string UnterminatedCommentDiagnosticType = "Preprocessor.UnterminatedComment";

        /// <summary>
        /// Perform necessary operations on input before passing it to lexer.
        /// Currently its only responsibility is removing comments, replacing
        /// them with space (supports nesting). Inserted space receives ILocation
        /// from comment's leftmost '/' (i.e. ILocation of first character of "/*...*/"),
        /// every other character (not commented out) keeps its original ILocation.
        /// </summary>
        /// <param name="input">Input data in form of pairs (location, character).</param>
        /// <param name="diagnostics">Logs in this object all errors</param>
        /// <returns>Input with comments filtered out.</returns>
        public IEnumerable<KeyValuePair<ILocation, char>>
            PreprocessInput(IEnumerable<KeyValuePair<ILocation, char>> input, IDiagnostics diagnostics)
        {
            return this.RemoveComments(input, diagnostics);
        }

        private IEnumerable<KeyValuePair<ILocation, char>>
            RemoveComments(IEnumerable<KeyValuePair<ILocation, char>> input, IDiagnostics diagnostics)
        {
            KeyValuePair<ILocation, char>? prev = null;
            int depth = 0;
            foreach (KeyValuePair<ILocation, char> c in input)
            {
                if (prev?.Value == '/' && c.Value == '*')
                {
                    if (depth++ == 0)
                    {
                        yield return new KeyValuePair<ILocation, char>(prev.Value.Key, ' ');
                    }

                    prev = null;
                    continue;
                }

                if (prev?.Value == '*' && c.Value == '/')
                {
                    if (depth == 0)
                    {
                        diagnostics.Add(new Diagnostic(
                            DiagnosticStatus.Error,
                            UnexpectedCommentEndDiagnosticType,
                            "Unexpected comment end delimiter at {0}",
                            new List<Lexer.Range> { new Lexer.Range(c.Key, c.Key) }));
                        throw new PreprocessorException("Unexpected comment end", c.Key);
                    }

                    prev = null;
                    depth--;
                    continue;
                }

                if (depth == 0 && prev.HasValue)
                {
                    yield return prev.Value;
                }

                prev = c;
            }

            if (depth != 0)
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticStatus.Error,
                    UnterminatedCommentDiagnosticType,
                    "Unterminated comment at the end of input",
                    new List<Lexer.Range> { }));
                throw new PreprocessorException("Non-terminated comment at the end of file", prev?.Key);
            }

            if (prev.HasValue)
            {
                yield return prev.Value;
            }
        }
    }
}
