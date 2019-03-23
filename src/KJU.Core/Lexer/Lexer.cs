namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Automata;
    using KJU.Core.Automata.NfaToDfa;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Regex;
    using KJU.Core.Regex.StringToRegexConverter;

    public class Lexer<TLabel>
    {
        public const string NonTokenDiagnosticType = "NonToken";
        public const string UnexpectedEOFDiagnosticType = "UnexpecetdEOF";

        private readonly IDfa<TLabel, char> minimalizedDfa;
        private readonly TLabel eof;
        private readonly TLabel noneValue;

        // tokenCategories - List of pair (Token, Regex for thie Token)
        public Lexer(
            IEnumerable<KeyValuePair<TLabel, string>> tokenCategories,
            TLabel eof,
            TLabel noneValue,
            Func<IEnumerable<TLabel>, TLabel> conflictSolver)
        {
            this.eof = eof;
            this.noneValue = noneValue;
            var converter = new StringToRegexConverterFactory().CreateConverter();
            Dictionary<TLabel, IDfa<bool, char>> multipleDfa = tokenCategories.ToDictionary(
                x => x.Key,
                x =>
                {
                    Regex<char> regex = converter.Convert(x.Value);
                    INfa<char> nfaPre = RegexToNfaConverter<char>.Convert(regex);
                    INfa<char> nfa = ConcreteNfa<char>.CreateFromNfa(nfaPre);
                    IDfa<bool, char> dfa = NfaToDfaConverter<char>.Convert(nfa);
                    return DfaMinimizer<bool, char>.Minimize(dfa);
                });
            var mergedDfa = DfaMerger<TLabel, char>.Merge(multipleDfa, conflictSolver);
            this.minimalizedDfa = DfaMinimizer<TLabel, char>.Minimize(mergedDfa);
        }

        public Lexer(IDfa<TLabel, char> dfa, TLabel eof)
        {
            this.minimalizedDfa = dfa;
            this.eof = eof;
        }

        public IEnumerable<Token<TLabel>> Scan(IEnumerable<KeyValuePair<ILocation, char>> text, IDiagnostics diagnostics)
        {
            using (var it = text.GetEnumerator())
            {
                it.MoveNext();
                var currChar = it.Current;
                var currState = this.minimalizedDfa.StartingState();
                currState = currChar.Value == Constants.EndOfInput
                    ? null
                    : this.minimalizedDfa.Transitions(currState)[currChar.Value];
                ILocation begin = currChar.Key;
                StringBuilder tokenText = new StringBuilder();
                while (currChar.Value != Constants.EndOfInput)
                {
                    it.MoveNext();
                    var nextChar = it.Current;
                    IState nextState = nextChar.Value == Constants.EndOfInput
                        ? null
                        : this.minimalizedDfa.Transitions(currState)[nextChar.Value];
                    tokenText.Append(currChar.Value);
                    if (nextState == null || this.minimalizedDfa.IsStable(nextState))
                    {
                        TLabel label = this.minimalizedDfa.Label(currState);
                        Range rng = new Range { Begin = begin, End = nextChar.Key };
                        if (label.Equals(this.noneValue))
                        {
                            diagnostics.Add(new Diagnostic(
                                DiagnosticStatus.Error,
                                NonTokenDiagnosticType,
                                $"Non-token at position {{0}} with text '{Diagnostic.EscapeForMessage(tokenText.ToString())}'",
                                new List<Range> { rng }));
                            throw new FormatException($"Non-token at position {rng} with text '{tokenText}'");
                        }

                        Token<TLabel> ret = new Token<TLabel>
                            { Category = label, InputRange = rng, Text = tokenText.ToString() };
                        tokenText.Clear();
                        begin = nextChar.Key;
                        nextState = this.minimalizedDfa.StartingState();
                        nextState = nextChar.Value == Constants.EndOfInput
                            ? null
                            : this.minimalizedDfa.Transitions(nextState)[nextChar.Value];
                        yield return ret;
                    }

                    currChar = nextChar;
                    currState = nextState;
                }

                if (begin != currChar.Key)
                {
                    diagnostics.Add(new Diagnostic(
                        DiagnosticStatus.Error,
                        UnexpectedEOFDiagnosticType,
                        "Unexpected end of input",
                        new List<Range> { }));
                    throw new FormatException("Unexpected EOF");
                }
            }

            yield return new Token<TLabel> { Category = this.eof };
        }
    }
}