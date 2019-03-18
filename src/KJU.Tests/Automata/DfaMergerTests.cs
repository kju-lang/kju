namespace KJU.Tests.Automata
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Util;

    [TestClass]
    public class DfaMergerTests
    {
        [TestMethod]
        public void TestSimple()
        {
            var a1 = CreateLetterPlusDfa('a'); // a+
            var a2 = CreateLetterPlusDfa('b'); // b+

            Assert.AreEqual(true, GetTargetLabel(a2, "bbb", false));
            Assert.AreEqual(false, GetTargetLabel(a2, string.Empty, false));

            var merged = DfaMerger<int, char>.Merge(
                new Dictionary<int, IDfa<bool, char>> { { 1, a1 }, { 2, a2 } },
                (labels) => !labels.Any() ? 0 : labels.Min());

            Assert.AreEqual(1, GetTargetLabel(merged, "a", 0));
            Assert.AreEqual(2, GetTargetLabel(merged, "b", 0));
            Assert.AreEqual(2, GetTargetLabel(merged, "bb", 0));
            Assert.AreEqual(2, GetTargetLabel(merged, "bbbbb", 0));
            Assert.AreEqual(1, GetTargetLabel(merged, "aaaaaaaaaa", 0));
            Assert.AreEqual(0, GetTargetLabel(merged, string.Empty, 0));
            Assert.AreEqual(0, GetTargetLabel(merged, "baa", 0));

            var merged2 = new ConcreteDfa<int, char>();
            merged2.AddEdge(0, 'a', 1);
            merged2.AddEdge(0, 'b', 2);

            merged2.AddEdge(1, 'a', 1);
            merged2.AddEdge(1, 'b', 3);

            merged2.AddEdge(2, 'a', 3);
            merged2.AddEdge(2, 'b', 2);

            merged2.AddEdge(3, 'a', 3);
            merged2.AddEdge(3, 'b', 3);

            merged2.Labels[0] = 0;
            merged2.Labels[1] = 1;
            merged2.Labels[2] = 2;
            merged2.Labels[3] = 0;

            Assert.IsTrue(DfaEquivalence<int, char>.AreEquivalent(merged, merged2));
        }

        [TestMethod]
        public void TestConflict()
        {
            var a1 = CreateLetterPlusDfa('a'); // a+

            var merged1 = DfaMerger<int, char>.Merge(
                new Dictionary<int, IDfa<bool, char>> { { 1, a1 }, { 2, a1 } },
                labels => !labels.Any() ? 0 : labels.Min());

            Assert.AreEqual(0, GetTargetLabel(merged1, string.Empty, 0));
            Assert.AreEqual(1, GetTargetLabel(merged1, "a", 0));
            Assert.AreEqual(0, GetTargetLabel(merged1, "b", 0));

            var a1Numeric = new ConcreteDfa<int, char>(); // a+
            a1Numeric.AddEdge(0, 'a', 1);
            a1Numeric.AddEdge(1, 'a', 1);
            a1Numeric.Labels[0] = 0;
            a1Numeric.Labels[1] = 1;

            Assert.IsTrue(DfaEquivalence<int, char>.AreEquivalent(merged1, a1Numeric));
        }

        [TestMethod]
        public void TestSimple2()
        {
            var a1 = new ConcreteDfa<bool, char>(); // ab

            a1.AddEdge(0, 'b', 3);
            a1.AddEdge(0, 'a', 1);
            a1.AddEdge(1, 'b', 2);
            a1.Labels[0] = false;
            a1.Labels[1] = false;
            a1.Labels[2] = true;
            a1.Labels[3] = false;

            var a2 = new ConcreteDfa<bool, char>(); // (ab)*
            a2.AddEdge(0, 'a', 1);
            a2.AddEdge(1, 'b', 0);
            a2.Labels[0] = true;
            a2.Labels[1] = false;

            var merged1 = DfaMerger<int, char>.Merge(
                new Dictionary<int, IDfa<bool, char>> { { 1, a1 }, { 2, a2 } },
                (labels) => !labels.Any() ? 0 : labels.Min());

            Assert.AreEqual(2, GetTargetLabel(merged1, string.Empty, 0));
            Assert.AreEqual(0, GetTargetLabel(merged1, "a", 0));
            Assert.AreEqual(1, GetTargetLabel(merged1, "ab", 0));
            Assert.AreEqual(2, GetTargetLabel(merged1, "abab", 0));
            Assert.AreEqual(2, GetTargetLabel(merged1, "ababab", 0));
            Assert.AreEqual(0, GetTargetLabel(merged1, "ababa", 0));

            Assert.IsTrue(DfaEquivalence<int, char>.AreEquivalent(merged1, merged1));

            var merged2 = new ConcreteDfa<int, char>();
            merged2.AddEdge(0, 'a', 1);
            merged2.AddEdge(1, 'b', 2);
            merged2.AddEdge(2, 'a', 3);
            merged2.AddEdge(3, 'b', 4);
            merged2.AddEdge(4, 'a', 3);

            merged2.AddEdge(0, 'b', 5);
            merged2.AddEdge(1, 'a', 5);
            merged2.AddEdge(2, 'b', 5);
            merged2.AddEdge(3, 'a', 5);
            merged2.AddEdge(4, 'b', 5);

            merged2.AddEdge(5, 'a', 5);
            merged2.AddEdge(5, 'b', 5);

            merged2.Labels[0] = 2;
            merged2.Labels[1] = 0;
            merged2.Labels[2] = 1;
            merged2.Labels[3] = 0;
            merged2.Labels[4] = 2;
            merged2.Labels[5] = 0;

            Assert.IsTrue(DfaEquivalence<int, char>.AreEquivalent(merged1, merged2));
        }

        private static ConcreteDfa<bool, char> CreateLetterPlusDfa(char letter)
        {
            var result = new ConcreteDfa<bool, char>(); // 'letter'+
            result.AddEdge(0, letter, 1);
            result.AddEdge(1, letter, 1);
            result.Labels[0] = false;
            result.Labels[1] = true;
            return result;
        }

        private static TLabel GetTargetLabel<TLabel>(IDfa<TLabel, char> dfa, string s, TLabel defaultLabel)
        {
            var state = dfa.StartingState();
            foreach (var ch in s)
            {
                var transitions = dfa.Transitions(state);
                if (!transitions.ContainsKey(ch))
                {
                    return defaultLabel;
                }

                state = transitions[ch];
            }

            return dfa.Label(state);
        }
    }
}