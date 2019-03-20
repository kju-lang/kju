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
        private readonly IDfa<TLabel, char> minimalizedDfa;
        private readonly TLabel noneValue;

        // tokenCategories - List of pair (Token, Regex for thie Token)
        public Lexer(IEnumerable<KeyValuePair<TLabel, string>> tokenCategories, TLabel noneValue, Func<IEnumerable<TLabel>, TLabel> conflictSolver)
        {
            this.noneValue = noneValue;
            var converter = new StringToRegexConverterFactory().CreateConverter();
            Dictionary<TLabel, IDfa<bool, char>> multipleDfa = tokenCategories.ToDictionary(
                x => x.Key,
                x =>
                {
                    Console.WriteLine($"compiling {x.Value}...");
                    Regex<char> regex = converter.Convert(x.Value);
                    Console.WriteLine($"compiling {x.Value}... 2");
                    INfa<char> nfaPre = RegexToNfaConverter<char>.Convert(regex);
                    Console.WriteLine($"compiling {x.Value}... 3a");
                    INfa<char> nfa = ConcreteNfa<char>.CreateFromNfa(nfaPre);
                    Console.WriteLine($"compiling {x.Value}... 3");
                    IDfa<bool, char> dfa = NfaToDfaConverter<char>.Convert(nfa);
                    Console.WriteLine($"compiling {x.Value}... 4");
                    return DfaMinimizer<bool, char>.Minimize(dfa);
                });
            var mergedDfa = DfaMerger<TLabel, char>.Merge(multipleDfa, conflictSolver);
            this.minimalizedDfa = DfaMinimizer<TLabel, char>.Minimize(mergedDfa);
        }

        public Lexer(IDfa<TLabel, char> dfa)
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
                currState = currChar.Value == Constants.EndOfInput ? null : this.minimalizedDfa.Transitions(currState)[currChar.Value];
                ILocation begin = currChar.Key;
                StringBuilder tokenText = new StringBuilder();
                while (currChar.Value != Constants.EndOfInput)
                {
                    it.MoveNext();
                    var nextChar = it.Current;
                    IState nextState = nextChar.Value == Constants.EndOfInput ? null : this.minimalizedDfa.Transitions(currState)[nextChar.Value];
                    tokenText.Append(currChar.Value);
                    if (nextState == null || this.minimalizedDfa.IsStable(nextState))
                    {
                        TLabel label = this.minimalizedDfa.Label(currState);
                        Range rng = new Range { Begin = begin, End = nextChar.Key };
                        if (label.Equals(this.noneValue))
                        {
                            throw new FormatException($"Non-token at position {rng} with text {tokenText}");
                        }

                        Token<TLabel> ret = new Token<TLabel> { Category = label, InputRange = rng, Text = tokenText.ToString() };
                        tokenText.Clear();
                        begin = nextChar.Key;
                        nextState = this.minimalizedDfa.StartingState();
                        nextState = nextChar.Value == Constants.EndOfInput ? null : this.minimalizedDfa.Transitions(nextState)[nextChar.Value];
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
