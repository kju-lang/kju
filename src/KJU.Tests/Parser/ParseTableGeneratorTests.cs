namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Parser;
    using KJU.Core.Regex;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Util;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class ParseTableGeneratorTests
    {
        private enum Label
        {
            A,
            B,
            C,
            EOF
        }

        [TestMethod]
        public void TestEmpty()
        {
            var grammar = new CompiledGrammar<Label>
                { Rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>>(), StartSymbol = Label.A };
            var follow = new Dictionary<Label, IReadOnlyCollection<DfaAndState<Label>>>();
            var firstPlus = new Dictionary<Label, IReadOnlyCollection<DfaAndState<Label>>>();
            var parseTable = ParseTableGenerator<Label>.Parse(grammar, follow, firstPlus);

            Assert.AreEqual(0, parseTable.Count);
        }

        [TestMethod]
        public void TestSimple()
        {
            var dfa = CreateLinearDfa(new List<Label> { Label.B, Label.C });

            // A -> BC
            var rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>> { { Label.A, dfa } };

            // regex matching the rule above
            var regex = Concat(Label.B.ToRegex(), Label.C.ToRegex());

            // state 2 is the only accepting state
            dfa.Labels[2] = Optional<Rule<Label>>.Some(new Rule<Label> { Lhs = Label.A, Rhs = regex });

            var grammar = new CompiledGrammar<Label> { Rules = rules, StartSymbol = Label.A };

            var follow = new Dictionary<Label, IReadOnlyCollection<DfaAndState<Label>>>
            {
                [Label.EOF] = new List<DfaAndState<Label>>
                {
                    new DfaAndState<Label> { Dfa = dfa, State = dfa.GetState(2) }
                }
            };

            var firstPlus = new Dictionary<Label, IReadOnlyCollection<DfaAndState<Label>>>
            {
                [Label.B] = new List<DfaAndState<Label>>
                {
                    new DfaAndState<Label> { Dfa = dfa, State = dfa.GetState(0) }
                },
                [Label.C] = new List<DfaAndState<Label>>
                {
                    new DfaAndState<Label> { Dfa = dfa, State = dfa.GetState(1) }
                }
            };

            var parseTable = ParseTableGenerator<Label>.Parse(grammar, follow, firstPlus);

            // compressed representation of the expected parse table
            var actions = new List<Tuple<int, Label, bool>>
            {
                Tuple.Create(0, Label.B, false),
                Tuple.Create(1, Label.C, false),
                Tuple.Create(2, Label.EOF, true)
            };

            var parseTableExpected =
                new Dictionary<Tuple<IDfa<Optional<Rule<Label>>, Label>, IState, Label>, ParseAction<Label>>();

            foreach (var(index, label, isReduce) in actions)
            {
                var state = dfa.GetState(index);
                var key = Tuple.Create<IDfa<Optional<Rule<Label>>, Label>, IState, Label>(dfa, state, label);
                var actionKind = isReduce
                    ? ParseAction<Label>.ActionKind.Reduce
                    : ParseAction<Label>.ActionKind.Shift;
                var actionLabel = new List<Label> { Label.B, Label.C, Label.A }.ElementAt(index);

                parseTableExpected[key] = new ParseAction<Label> { Kind = actionKind, Label = actionLabel };
            }

            Assert.IsTrue(MappingEquivalence.AreEquivalent(parseTable, parseTableExpected));
        }

        [TestMethod]
        public void TestConflict()
        {
            var dfa = CreateLinearDfa(new List<Label> { Label.A, Label.A });

            // A -> AA
            var rules = new Dictionary<Label, IDfa<Optional<Rule<Label>>, Label>> { { Label.A, dfa } };

            // regex matching the rule above
            var regex = Concat(Label.A.ToRegex(), Label.A.ToRegex());

            // state 2 is the only accepting state
            dfa.Labels[2] = Optional<Rule<Label>>.Some(new Rule<Label> { Lhs = Label.A, Rhs = regex });

            var grammar = new CompiledGrammar<Label> { Rules = rules, StartSymbol = Label.A };

            var follow = new Dictionary<Label, IReadOnlyCollection<DfaAndState<Label>>>
            {
                [Label.EOF] = new List<DfaAndState<Label>>
                {
                    new DfaAndState<Label> { Dfa = dfa, State = dfa.GetState(2) }
                }
            };

            var firstPlus = new Dictionary<Label, IReadOnlyCollection<DfaAndState<Label>>>
            {
                [Label.A] = new List<DfaAndState<Label>>
                {
                    new DfaAndState<Label> { Dfa = dfa, State = dfa.GetState(0) },
                    new DfaAndState<Label> { Dfa = dfa, State = dfa.GetState(1) }
                }
            };

            Assert.ThrowsException<InvalidOperationException>(() =>
                ParseTableGenerator<Label>.Parse(grammar, follow, firstPlus));
        }

        private static Dfa CreateLinearDfa(IReadOnlyList<Label> sequence)
        {
            var numStates = sequence.Count + 2;
            var allLabels = (Label[])Enum.GetValues(typeof(Label));

            var dfa = new Dfa(numStates);
            for (var start = 0; start < numStates; start++)
            {
                foreach (var label in allLabels)
                {
                    var end = (start < sequence.Count && sequence[start].Equals(label)) ? start + 1 : numStates - 1;
                    dfa.Edges[start][label] = end;
                }
            }

            return dfa;
        }

        private class Dfa : IDfa<Optional<Rule<Label>>, Label>
        {
            public Dfa(int numStates)
            {
                this.Labels = Enumerable.Range(0, numStates).Select(x => Optional<Rule<Label>>.None()).ToList();
                this.Edges = Enumerable.Range(0, numStates).Select(x => new Dictionary<Label, int>()).ToList();
            }

            public List<Optional<Rule<Label>>> Labels { get; }

            public List<Dictionary<Label, int>> Edges { get; }

            public bool IsStable(IState state)
            {
                var i = ((ValueState<int>)state).Value;
                return this.Edges[i].Values.All(i.Equals);
            }

            public IState GetState(int i)
            {
                return new ValueState<int>(i);
            }

            public Optional<Rule<Label>> Label(IState state)
            {
                return this.Labels[((ValueState<int>)state).Value];
            }

            public IState StartingState()
            {
                return new ValueState<int>(0);
            }

            public IReadOnlyDictionary<Label, IState> Transitions(IState state)
            {
                var i = ((ValueState<int>)state).Value;
                return this.Edges[i].ToDictionary(kv => kv.Key, kv => new ValueState<int>(kv.Value) as IState);
            }

            public IState Transition(IState state, Label symbol)
            {
                if (this.Transitions(state).TryGetValue(symbol, out var newState))
                {
                    return newState;
                }

                return null;
            }
        }
    }
}