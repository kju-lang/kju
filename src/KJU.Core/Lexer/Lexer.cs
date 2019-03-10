namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Automata;
    using KJU.Core.Automata.NfaToDfa;
    using KJU.Core.Input;
    using KJU.Core.Regex;
    using KJU.Core.Regex.StringToRegexConverter;

    public class Lexer<TLabel>
    {
        private readonly IDfa<TLabel> minimalizedDfa;

        // tokenCategories - List of pair (Token, Regex for thie Token)
        public Lexer(IEnumerable<KeyValuePair<TLabel, string>> tokenCategories, Func<IEnumerable<TLabel>, TLabel> conflictSolver)
        {
            var converter = new StringToRegexConverterFactory().CreateConverter();
            Dictionary<TLabel, IDfa<bool>> multipleDfa = tokenCategories.ToDictionary(
                x => x.Key,
                x =>
                {
                    Regex regex = converter.Convert(x.Value);
                    INfa nfa = RegexToNfaConverter.Convert(regex);
                    IDfa<bool> dfa = NfaToDfaConverter.Convert(nfa);
                    return DfaMinimizer<bool>.Minimize(dfa);
                });
            var mergedDfa = DfaMerger<TLabel>.Merge(multipleDfa, conflictSolver);
            this.minimalizedDfa = DfaMinimizer<TLabel>.Minimize(mergedDfa);
        }

        public Lexer(IDfa<TLabel> dfa)
        {
            this.minimalizedDfa = dfa;
        }

        public IEnumerable<Token<TLabel>> Scan(IEnumerable<KeyValuePair<ILocation, char>> text)
        {
            using (var it = text.GetEnumerator())
            {
                it.MoveNext();
                var currChar = it.Current;
                var currState = this.minimalizedDfa.StartingState();
                currState = this.minimalizedDfa.Transitions(currState)[currChar.Value];
                ILocation begin = currChar.Key;
                StringBuilder tokenText = new StringBuilder();
                while (currChar.Value != Constants.EndOfInput)
                {
                    it.MoveNext();
                    var nextChar = it.Current;
                    var nextState = this.minimalizedDfa.Transitions(currState)[nextChar.Value];
                    tokenText.Append(currChar.Value);
                    if (this.minimalizedDfa.IsStable(nextState))
                    {
                        TLabel label = this.minimalizedDfa.Label(currState);
                        Range rng = new Range { Begin = begin, End = nextChar.Key };
                        if (label == null)
                        {
                            throw new FormatException($"Non-token at position {rng} with text {tokenText}");
                        }

                        Token<TLabel> ret = new Token<TLabel> { Category = label, InputRange = rng, Text = tokenText.ToString() };
                        tokenText.Clear();
                        begin = nextChar.Key;
                        nextState = this.minimalizedDfa.StartingState();
                        nextState = this.minimalizedDfa.Transitions(nextState)[nextChar.Value];
                        yield return ret;
                    }

                    currChar = nextChar;
                    currState = nextState;
                }

                if (begin != currChar.Key)
                {
                    throw new FormatException("Unexpected EOF");
                }
            }
        }
    }
}
