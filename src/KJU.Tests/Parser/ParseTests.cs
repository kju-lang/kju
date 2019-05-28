namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Core.Regex;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Util;

    [TestClass]
    public class ParseTests
    {
        private enum Label
        {
            A,
            B,
            Eof
        }

        [TestMethod]
        public void TestParseException()
        {
            var rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>>();
            var dfa = new IntDfa();
            rules.Add(Label.A, dfa);
            var grammar = new CompiledGrammar<Label>() { Rules = rules, StartSymbol = Label.A };
            var parseTable =
                new Dictionary<Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>, ParseAction<Label>>
                {
                    {
                        new Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>(dfa, new IntState(0), Label.B),
                        new ParseAction<Label>() { Kind = ParseAction<Label>.ActionKind.Shift }
                    }
                };
            var parser = new Parser<Label>(grammar, parseTable);

            var tokens = new List<Token<Label>> { new Token<Label>() { Category = Label.B } };
            var diag = new Moq.Mock<IDiagnostics>();

            Assert.ThrowsException<ParseException>(() => parser.Parse(tokens, diag.Object));
            MockDiagnostics.Verify(diag, Parser<Label>.PrematureEofDiagnosticType);
        }

        [TestMethod]
        public void TestInputRangeEmpty()
        {
            var dfa = new EpsDfa();
            var rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>> { { Label.A, dfa } };
            var grammar = new CompiledGrammar<Label>() { Rules = rules, StartSymbol = Label.A };
            var parseTable =
                new Dictionary<Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>, ParseAction<Label>>
                {
                    {
                        new Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>(dfa, new IntState(0), Label.B),
                        new ParseAction<Label>() { Kind = ParseAction<Label>.ActionKind.Reduce }
                    }
                };
            var parser = new Parser<Label>(grammar, parseTable);

            var tokens = new List<Token<Label>> { new Token<Label>() { Category = Label.B } };
            var root = parser.Parse(tokens, null);
            Assert.IsNull(root.InputRange);
        }

        [TestMethod]
        public void TestInputRange()
        {
            var rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>>();
            var dfa = new IntDfa();
            rules.Add(Label.A, dfa);
            var grammar = new CompiledGrammar<Label>() { Rules = rules, StartSymbol = Label.A };
            var parseTable =
                new Dictionary<Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>, ParseAction<Label>>
                {
                    {
                        new Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>(dfa, new IntState(0), Label.B),
                        new ParseAction<Label> { Kind = ParseAction<Label>.ActionKind.Shift }
                    },
                    {
                        new Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>(dfa, new IntState(0), Label.Eof),
                        new ParseAction<Label> { Kind = ParseAction<Label>.ActionKind.Reduce }
                    }
                };
            var parser = new Parser<Label>(grammar, parseTable);

            var tokenRange = new Range(new StringLocation(0), new StringLocation(1));
            var tokens = new List<Token<Label>>
            {
                new Token<Label>
                {
                    Category = Label.B,
                    InputRange = tokenRange
                },
                new Token<Label>
                {
                    Category = Label.Eof
                }
            };
            var root = parser.Parse(tokens, null);
            Assert.AreEqual(tokenRange, root.InputRange);
        }

        private class IntState : IState
        {
            private readonly int i;

            public IntState(int i)
            {
                this.i = i;
            }

            public bool Equals(IState other)
            {
                return this.Equals(other as object);
            }

            public override bool Equals(object other)
            {
                if (other is IntState otherIntState)
                {
                    return this.i == otherIntState.i;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return this.i;
            }
        }

        private class IntDfa : IDfa<Optional<Rule<Label>>, Label>
        {
            public bool IsStable(IState state)
            {
                return false;
            }

            public Optional<Rule<Label>> Label(IState state)
            {
                return Optional<Rule<Label>>.Some(new Rule<Label>
                    { Lhs = ParseTests.Label.A, Rhs = ParseTests.Label.B.ToRegex() });
            }

            public IState StartingState()
            {
                return new IntState(0);
            }

            public IState Transition(IState state, Label symbol)
            {
                if (this.Transitions(state).TryGetValue(symbol, out var newState))
                {
                    return newState;
                }

                return null;
            }

            public IReadOnlyDictionary<Label, IState> Transitions(IState state)
            {
                var edges = new Dictionary<Label, IState>
                {
                    { ParseTests.Label.A, new IntState(0) }, { ParseTests.Label.B, new IntState(0) }
                };
                return edges;
            }
        }

        private class EpsDfa : IDfa<Optional<Rule<Label>>, Label>
        {
            public bool IsStable(IState state)
            {
                return false;
            }

            public Optional<Rule<Label>> Label(IState state)
            {
                return Optional<Rule<Label>>.Some(new Rule<Label>());
            }

            public IState StartingState()
            {
                return new IntState(0);
            }

            public IState Transition(IState state, Label symbol)
            {
                if (this.Transitions(state).TryGetValue(symbol, out var newState))
                {
                    return newState;
                }

                return null;
            }

            public IReadOnlyDictionary<Label, IState> Transitions(IState state)
            {
                var edges = new Dictionary<Label, IState>
                {
                    { ParseTests.Label.A, new IntState(0) }, { ParseTests.Label.B, new IntState(0) }
                };
                return edges;
            }
        }
    }
}