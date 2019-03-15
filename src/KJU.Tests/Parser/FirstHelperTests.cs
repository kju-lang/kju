namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Parser;
    using KJU.Core.Regex;
    using KJU.Core.Util;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FirstHelperTests
    {
        [TestMethod]
        public void SingleDfaTest()
        {
            var dfa = new Dfa<string>();
            dfa.AddEdge(0, "A", 2);
            dfa.AddEdge(0, "B", 1);
            dfa.AddEdge(1, "B", 2);

            var rules = new Dictionary<string, IDfa<Optional<Rule<string>>, string>>
            {
                ["A"] = dfa
            };

            var grammar = new CompiledGrammar<string> { Rules = rules };

            var nullables = new List<DfaAndState<string>>
            {
                new DfaAndState<string> { Dfa = dfa, State = new ValueState<int>(0) }
            };

            var firstSymbols = FirstHelper<string>.GetFirstSymbols(grammar, nullables);

            var expected = new string[]
            {
                "A,B",
                "B",
                string.Empty
            };
            for (int i = 0; i < 3; ++i)
            {
                var stateEntity = new DfaAndState<string> { Dfa = dfa, State = new ValueState<int>(i) };
                string output = string.Join(',', firstSymbols[stateEntity].OrderBy(x => x));
                Assert.AreEqual(expected[i], output, $"Unexpected output on test {i}: expected is [{expected[i]}], but found [{output}]");
            }
        }

        [TestMethod]
        public void ThreeDfasTest()
        {
            var firstDfa = new Dfa<string>();
            firstDfa.AddEdge(0, "D", 1);
            firstDfa.AddEdge(0, "F", 4);
            firstDfa.AddEdge(0, "C", 3);
            firstDfa.AddEdge(0, "B", 2);
            firstDfa.AddEdge(2, "B", 7);
            firstDfa.AddEdge(3, "A", 5);
            firstDfa.AddEdge(4, "G", 6);
            firstDfa.AddEdge(6, "A", 5);

            var secondDfa = new Dfa<string>();
            secondDfa.AddEdge(0, "B", 1);
            secondDfa.AddEdge(0, "C", 2);
            secondDfa.AddEdge(2, "A", 3);
            secondDfa.AddEdge(1, "E", 4);

            var thirdDfa = new Dfa<string>();

            var rules = new Dictionary<string, IDfa<Optional<Rule<string>>, string>>
            {
                ["A"] = firstDfa,
                ["B"] = secondDfa,
                ["C"] = thirdDfa
            };

            var grammar = new CompiledGrammar<string> { Rules = rules };

            var nullables = new List<DfaAndState<string>>
            {
                new DfaAndState<string> { Dfa = thirdDfa, State = new ValueState<int>(0) }
            };

            var firstSymbols = FirstHelper<string>.GetFirstSymbols(grammar, nullables);

            var expected = new string[][]
            {
                new string[] { "A,B,C,D,F", string.Empty, "A,B,C,D,F", "A,B,C,D,F", "G", string.Empty, "A,B,C,D,F", string.Empty },
                new string[] { "A,B,C,D,F", "E", "A,B,C,D,F", string.Empty, string.Empty },
                new string[] { string.Empty }
            };
            var size = new int[] { 8, 5, 1 };
            var dfas = new ConcreteDfa<Optional<Rule<string>>, string>[] { firstDfa, secondDfa, thirdDfa };

            for (int test = 0; test < 3; ++test)
            {
                for (int i = 0; i < size[test]; ++i)
                {
                    var stateEntity = new DfaAndState<string> { Dfa = dfas[test], State = new ValueState<int>(i) };
                    string output = string.Join(',', firstSymbols[stateEntity].OrderBy(x => x));
                    Assert.AreEqual(expected[test][i], output, $"Unexpected output on test ({test}, {i}): expected is [{expected[test][i]}], but found [{output}]");
                }
            }
        }

        [TestMethod]
        public void DeadStatesTest()
        {
            var dfa = new Dfa<string>();
            dfa.AddEdge(0, "A", 3);
            dfa.AddEdge(1, "B", 3);
            dfa.AddEdge(2, "C", 3);
            dfa.AddEdge(0, "E", 1);
            dfa.AddEdge(1, "E", 2);

            dfa.AddBadState(3);

            var emptyDfa = new Dfa<string>();

            var rules = new Dictionary<string, IDfa<Optional<Rule<string>>, string>>
            {
                ["A"] = dfa,
                ["E"] = emptyDfa
            };

            var grammar = new CompiledGrammar<string> { Rules = rules };

            var nullables = new List<DfaAndState<string>>
            {
                new DfaAndState<string> { Dfa = emptyDfa, State = new ValueState<int>(0) }
            };

            var firstSymbols = FirstHelper<string>.GetFirstSymbols(grammar, nullables);

            var expected = new string[]
            {
                "E",
                "E",
                string.Empty,
                string.Empty
            };
            for (int i = 0; i < 4; ++i)
            {
                var stateEntity = new DfaAndState<string> { Dfa = dfa, State = new ValueState<int>(i) };
                string output = string.Join(',', firstSymbols[stateEntity].OrderBy(x => x));
                Assert.AreEqual(expected[i], output, $"Unexpected output on test {i}: expected is [{expected[i]}], but found [{output}]");
            }
        }

        private class Dfa<Symbol> : ConcreteDfa<Optional<Rule<string>>, Symbol>, IDfa<Optional<Rule<string>>, Symbol>
        {
            private HashSet<IState> badStates = new HashSet<IState>();

            Optional<Rule<string>> IDfa<Optional<Rule<string>>, Symbol>.Label(IState state)
            {
                return !this.badStates.Contains(state) ? Optional<Rule<string>>.Some(new Rule<string>()) : Optional<Rule<string>>.None();
            }

            bool IDfa<Optional<Rule<string>>, Symbol>.IsStable(IState state)
            {
                return this.badStates.Contains(state);
            }

            public void AddBadState(int id)
            {
                this.badStates.Add(new ValueState<int>(id));
            }
        }
    }
}
