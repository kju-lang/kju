namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using KJU.Core;
    using KJU.Core.Automata;
    using KJU.Core.Automata.NfaToDfa;
    using KJU.Core.Diagnostics;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Core.Util;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ParseTests
    {
        private enum Label
        {
            A,
            B
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
            var diag = new Mock<IDiagnostics>();

            Assert.ThrowsException<ParseException>(() => parser.Parse(tokens, diag.Object));
            MockDiagnostics.Verify(diag, Parser<Label>.PrematureEOFDiagnosticType);
        }

        [TestMethod]
        public void TestEmpty()
        {
            var rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>>();
            var dfa = new EpsDfa();
            rules.Add(Label.A, dfa);
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
            ParseTree<Label> root = parser.Parse(tokens, null);
            Assert.IsNull(root.InputRange);
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
                return Optional<Rule<Label>>.None();
            }

            public IState StartingState()
            {
                return new IntState(0);
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
