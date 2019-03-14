namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Parser;
    using KJU.Core.Util;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ParserHelperTests
    {
        private static readonly Optional<Rule<char>> REJECT = Optional<Rule<char>>.None();
        private static readonly Optional<Rule<char>> ACCEPT = Optional<Rule<char>>.Some(new Rule<char>());

        [TestMethod]
        public void Test0SimpleParens()
        {
            // var rules = new List<Rule<char>>();
            var rules = new Dictionary<char, IDfa<Optional<Rule<char>>, char>>();

            // vvar grammar = new Grammar<char> { Rules = rules, StartSymbol = 'S' };
            // CompiledGrammar<char> compiledGrammar = GrammarCompiler<char>.CompileGrammar(grammar);
            var dfa = new ConcreteDfa<Optional<Rule<char>>, char>();
            dfa.AddEdge(0, '(', 1);
            dfa.AddEdge(1, 'S', 2);
            dfa.AddEdge(2, ')', 3);
            dfa.Labels.Add(0, ACCEPT);
            dfa.Labels.Add(1, REJECT);
            dfa.Labels.Add(2, REJECT);
            dfa.Labels.Add(3, ACCEPT);
            rules.Add('S', dfa);
            var grammar = new CompiledGrammar<char> { Rules = rules, StartSymbol = 'S' };
            var nullables = NullablesHelper<char>.GetNullableSymbols(grammar).ToList();
            {
                Assert.AreEqual(2, nullables.Count);
                var states = nullables.Select(x => ((ValueState<int>)x.State).Value).ToList();
                states.Sort();
                Assert.AreEqual(0, states[0]);
                Assert.AreEqual(3, states[1]);
            }

            var first = FirstHelper<char>.GetFirstSymbols(grammar, nullables);
            {
                var output = first.OrderBy(x => (x.Key.State as ValueState<int>).Value).ToList();

                var symbols = output[0].Value.ToList();
                Assert.AreEqual(1, symbols.Count);
                Assert.AreEqual('(', symbols[0]);

                symbols = output[1].Value.ToList();
                symbols.Sort();
                Assert.AreEqual(3, symbols.Count);
                Assert.AreEqual('(', symbols[0]);
                Assert.AreEqual(')', symbols[1]);
                Assert.AreEqual('S', symbols[2]);

                symbols = output[2].Value.ToList();
                Assert.AreEqual(1, symbols.Count);
                Assert.AreEqual(')', symbols[0]);

                symbols = output[3].Value.ToList();
                Assert.AreEqual(0, symbols.Count);
            }

            var follow = FollowHelper<char>.GetFollowSymbols(grammar, nullables, first.InverseRelation(), '\uffff');
            {
                var output = follow.OrderBy(x => (x.Key.State as ValueState<int>).Value).ToList();

                var symbols = output[0].Value.ToList();
            }
        }

        [TestMethod]
        public void Test1()
        {
            // Z –> d | XYZ
            // Y –> c | a
            // X –> Y | ε

            var rules = new Dictionary<char, IDfa<Optional<Rule<char>>, char>>();

            var zdfa = new ConcreteDfa<Optional<Rule<char>>, char>();
            zdfa.Magic = 0;
            zdfa.AddEdge(0, 'X', 1);
            zdfa.AddEdge(1, 'Y', 2);
            zdfa.AddEdge(2, 'Z', 3);
            zdfa.AddEdge(0, 'd', 3);
            zdfa.Labels.Add(0, REJECT);
            zdfa.Labels.Add(1, REJECT);
            zdfa.Labels.Add(2, REJECT);
            zdfa.Labels.Add(3, ACCEPT);
            rules.Add('Z', zdfa);

            var ydfa = new ConcreteDfa<Optional<Rule<char>>, char>();
            ydfa.Magic = 1;
            ydfa.AddEdge(0, 'c', 1);
            ydfa.AddEdge(0, 'a', 1);
            ydfa.Labels.Add(0, REJECT);
            ydfa.Labels.Add(1, ACCEPT);
            rules.Add('Y', ydfa);

            var xdfa = new ConcreteDfa<Optional<Rule<char>>, char>();
            xdfa.Magic = 2;
            xdfa.AddEdge(0, 'Y', 1);
            xdfa.Labels.Add(0, ACCEPT);
            xdfa.Labels.Add(1, ACCEPT);
            rules.Add('X', xdfa);

            var grammar = new CompiledGrammar<char> { Rules = rules, StartSymbol = 'Z' };
            var nullables = NullablesHelper<char>.GetNullableSymbols(grammar);
            {
                var output = nullables.OrderBy(x => (x.Dfa as ConcreteDfa<Optional<Rule<char>>, char>).Magic).ThenBy(x => (x.State as ValueState<int>).Value).ToList();
                Assert.AreEqual(4, output.Count);
                Assert.AreEqual(0, (output[0].Dfa as ConcreteDfa<Optional<Rule<char>>, char>).Magic);
                Assert.AreEqual(3, (output[0].State as ValueState<int>).Value);
                Assert.AreEqual(1, (output[1].Dfa as ConcreteDfa<Optional<Rule<char>>, char>).Magic);
                Assert.AreEqual(1, (output[1].State as ValueState<int>).Value);
                Assert.AreEqual(2, (output[2].Dfa as ConcreteDfa<Optional<Rule<char>>, char>).Magic);
                Assert.AreEqual(0, (output[2].State as ValueState<int>).Value);
                Assert.AreEqual(2, (output[3].Dfa as ConcreteDfa<Optional<Rule<char>>, char>).Magic);
                Assert.AreEqual(1, (output[3].State as ValueState<int>).Value);
            }

            var first = FirstHelper<char>.GetFirstSymbols(grammar, nullables);
            {
                var output = first.OrderBy(x => (x.Key.Dfa as ConcreteDfa<Optional<Rule<char>>, char>).Magic).ThenBy(x => (x.Key.State as ValueState<int>).Value).ToList();

                var symbols = output[0].Value.ToList(); // Z0
                symbols.Sort();
                Assert.AreEqual(5, symbols.Count);
                Assert.AreEqual('X', symbols[0]);
                Assert.AreEqual('Y', symbols[1]);
                Assert.AreEqual('a', symbols[2]);
                Assert.AreEqual('c', symbols[3]);
                Assert.AreEqual('d', symbols[4]);

                symbols = output[1].Value.ToList(); // Z1
                symbols.Sort();
                Assert.AreEqual(3, symbols.Count);
                Assert.AreEqual('Y', symbols[0]);
                Assert.AreEqual('a', symbols[1]);
                Assert.AreEqual('c', symbols[2]);

                symbols = output[2].Value.ToList(); // Z2
                symbols.Sort();
                Assert.AreEqual(6, symbols.Count);
                Assert.AreEqual('X', symbols[0]);
                Assert.AreEqual('Y', symbols[1]);
                Assert.AreEqual('Z', symbols[2]);
                Assert.AreEqual('a', symbols[3]);
                Assert.AreEqual('c', symbols[4]);
                Assert.AreEqual('d', symbols[5]);

                symbols = output[3].Value.ToList(); // Z3
                Assert.AreEqual(0, symbols.Count);

                symbols = output[4].Value.ToList(); // Y0
                symbols.Sort();
                Assert.AreEqual(2, symbols.Count);
                Assert.AreEqual('a', symbols[0]);
                Assert.AreEqual('c', symbols[1]);

                symbols = output[5].Value.ToList(); // Y1
                Assert.AreEqual(0, symbols.Count);

                symbols = output[6].Value.ToList(); // X0
                symbols.Sort();
                Assert.AreEqual(3, symbols.Count);
                Assert.AreEqual('Y', symbols[0]);
                Assert.AreEqual('a', symbols[1]);
                Assert.AreEqual('c', symbols[2]);

                symbols = output[7].Value.ToList(); // X1
                Assert.AreEqual(0, symbols.Count);
            }

                var follow = FollowHelper<char>.GetFollowSymbols(grammar, nullables, first.InverseRelation(), '\uffff');
             {
                 var output = follow.OrderBy(x => (x.Key.State as ValueState<int>).Value).ToList();

                 var symbols = output[0].Value.ToList();
             }
        }
    }
}