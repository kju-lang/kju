namespace KJU.Tests.Parser
{
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Parser;
    using KJU.Core.Regex;
    using KJU.Core.Util;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NullablesHelperTest
    {
        private enum Tag
        {
            A,
            B
        }

        [TestMethod]
        public void SingleDfaTest()
        {
            var grammar = new CompiledGrammar<Tag>();
            var rules = new Dictionary<Tag, IDfa<Optional<Rule<Tag>>, Tag>>();

            var dfa = new ConcreteDfa<Optional<Rule<Tag>>, Tag>();
            dfa.AddEdge(0, Tag.A, 1);
            dfa.AddEdge(1, Tag.A, 2);
            dfa.AddEdge(1, Tag.B, 1);
            dfa.AddEdge(2, Tag.A, 2);

            dfa.Labels.Add(0, Optional<Rule<Tag>>.None());
            dfa.Labels.Add(1, Optional<Rule<Tag>>.None());
            var r = new Rule<Tag>
            {
                Lhs = Tag.A
            };
            dfa.Labels.Add(2, Optional<Rule<Tag>>.Some(r));

            rules.Add(Tag.A, dfa);
            grammar.Rules = rules;

            var output = NullablesHelper<Tag>.GetNullableSymbols(grammar);
            Assert.AreEqual(1, output.Count);
        }

        [TestMethod]
        public void AlmostAllNullable()
        {
            var dfa1 = new ConcreteDfa<Optional<Rule<Tag>>, Tag>();
            dfa1.AddEdge(0, Tag.B, 0);
            dfa1.AddEdge(0, Tag.A, 1);

            var rules1 = new Rule<Tag>()
            {
                Lhs = Tag.A,
                Rhs = new ConcatRegex<Tag>(new AtomicRegex<Tag>(Tag.B), new AtomicRegex<Tag>(Tag.A))
            };
            dfa1.Labels.Add(0, Optional<Rule<Tag>>.Some(rules1));
            var rules2 = new Rule<Tag>
            {
                Lhs = Tag.A,
                Rhs = new ConcatRegex<Tag>(new AtomicRegex<Tag>(Tag.B), new AtomicRegex<Tag>(Tag.B))
            };
            dfa1.Labels.Add(1, Optional<Rule<Tag>>.Some(rules2));

            var dfa2 = new ConcreteDfa<Optional<Rule<Tag>>, Tag>();
            dfa2.AddEdge(0, Tag.A, 1);
            dfa2.AddEdge(0, Tag.B, 0);
            dfa2.AddEdge(0, Tag.B, 3);
            dfa2.AddEdge(1, Tag.A, 2);
            dfa2.AddEdge(2, Tag.A, 2);
            dfa2.AddEdge(2, Tag.B, 2);

            var rules3 = new Rule<Tag>()
            {
                Lhs = Tag.B,
                Rhs = new ConcatRegex<Tag>(new AtomicRegex<Tag>(Tag.A), new AtomicRegex<Tag>(Tag.A))
            };
            dfa2.Labels.Add(0, Optional<Rule<Tag>>.None());
            dfa2.Labels.Add(1, Optional<Rule<Tag>>.None());
            dfa2.Labels.Add(2, Optional<Rule<Tag>>.Some(rules3));
            dfa2.Labels.Add(3, Optional<Rule<Tag>>.None());

            var grammar = new CompiledGrammar<Tag>();
            var rules = new Dictionary<Tag, IDfa<Optional<Rule<Tag>>, Tag>> { { Tag.A, dfa1 }, { Tag.B, dfa2 } };
            grammar.Rules = rules;
            var nullables = NullablesHelper<Tag>.GetNullableSymbols(grammar);
            Assert.AreEqual(5, nullables.Count);
        }
    }
}