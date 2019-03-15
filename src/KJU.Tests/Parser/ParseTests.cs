namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using KJU.Core;
    using KJU.Core.Automata;
    using KJU.Core.Automata.NfaToDfa;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ParseTests
    {
        private enum Label
        {
            A, B
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
                if (!(other is IntState))
                {
                    return false;
                }

                return this.i == (other as IntState).i;
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
                IntState acc = new IntState(0);
                return acc;
            }

            public IReadOnlyDictionary<Label, IState> Transitions(IState state)
            {
                Dictionary<Label, IState> edges = new Dictionary<Label, IState>();
                Label l = ParseTests.Label.A;
                edges.Add(l, new IntState(0));
                l = ParseTests.Label.B;
                edges.Add(l, new IntState(0));
                return edges;
            }
        }

        [TestMethod]
#pragma warning disable SA1201 // Elements must appear in the correct order
        public void ParseTest()
#pragma warning restore SA1201 // Elements must appear in the correct order
        {
            var rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>>();
            IntDfa dfa = new IntDfa();
            rules.Add(Label.A, dfa);
            var grammar = new CompiledGrammar<Label>() { Rules = rules, StartSymbol = Label.A };
            var parseTable = new Dictionary<Tuple<IState, IDfa<Optional<Rule<Label>>, Label>, Label>, ParseAction<Label>>();
            parseTable.Add(new Tuple<IState, IDfa<Optional<Rule<Label>>, Label>, Label>(new IntState(0), dfa, Label.B), new ParseAction<Label>() { Kind = ParseAction<Label>.ActionKind.Shift });
            Parser<Label> parser = new Parser<Label>(grammar, parseTable);

            List<Token<Label>> tokens = new List<Token<Label>>();
            tokens.Add(new Token<Label>() { Category = Label.B });
            try
            {
                var root = parser.Parse(tokens);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "Invalid reduce action");
            }
        }
    }
}
