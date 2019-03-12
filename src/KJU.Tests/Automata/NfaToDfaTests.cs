namespace KJU.Tests.Automata
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using KJU.Core;
    using KJU.Core.Automata;
    using KJU.Core.Automata.NfaToDfa;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NfaToDfaTests
    {
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

        private class IntNfa : INfa<char>
        {
            public IReadOnlyCollection<IState> EpsilonTransitions(IState state)
            {
                IntState s = new IntState(0);
                HashSet<IState> set = new HashSet<IState>();
                set.Add(s);
                return set;
            }

            public bool IsAccepting(IState state)
            {
                IntState acc = new IntState(1);
                return acc.Equals(state);
            }

            public IState StartingState()
            {
                IntState acc = new IntState(0);
                return acc;
            }

            public IReadOnlyDictionary<char, IReadOnlyCollection<IState>> Transitions(IState state)
            {
                Dictionary<char, IReadOnlyCollection<IState>> edges = new Dictionary<char, IReadOnlyCollection<IState>>();
                if (!(state is IntState))
                {
                    return edges;
                }

                IntState s = new IntState(0);
                IntState t = new IntState(1);
                HashSet<IState> set = new HashSet<IState>();
                set.Add(s);
                set.Add(t);
                IReadOnlyCollection<IState> col = set;
                edges.Add('a', col);
                return edges;
            }
        }

        [TestMethod]
#pragma warning disable SA1201 // Elements must appear in the correct order
        public void TestNfaToDfa()
#pragma warning restore SA1201 // Elements must appear in the correct order
        {
            INfa<char> nfa = new IntNfa();

            IDfa<bool, char> dfa = NfaToDfaConverter<char>.Convert(nfa);

            IState start = dfa.StartingState();
            Assert.IsFalse(dfa.Label(start));
            IReadOnlyDictionary<char, IState> edges = dfa.Transitions(start);
            Assert.AreEqual(1, edges.Count);
            IState s = edges['a'];
            edges = dfa.Transitions(s);
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual(edges['a'], s);
            Assert.IsTrue(dfa.Label(s));
        }

        [TestMethod]
#pragma warning disable SA1201 // Elements must appear in the correct order
        public void TestNfaToDfaBigger()
#pragma warning restore SA1201 // Elements must appear in the correct order
        {
            List<IState> states = new List<IState>();
            for (int i = 0; i < 6; ++i)
            {
                IntState m = new IntState(i);
                states.Add(m);
            }

            Debug.WriteLine(states[0].Equals(states[0]));

            Dictionary<char, IReadOnlyCollection<IState>> trans0 = new Dictionary<char, IReadOnlyCollection<IState>>();
            trans0['a'] = new HashSet<IState> { states[1], states[2] };
            trans0['b'] = new HashSet<IState> { states[2], states[3] };

            Dictionary<char, IReadOnlyCollection<IState>> trans1 = new Dictionary<char, IReadOnlyCollection<IState>>();
            trans1['c'] = new HashSet<IState> { states[2] };

            Dictionary<char, IReadOnlyCollection<IState>> trans2 = new Dictionary<char, IReadOnlyCollection<IState>>();

            Dictionary<char, IReadOnlyCollection<IState>> trans3 = new Dictionary<char, IReadOnlyCollection<IState>>();
            Dictionary<char, IReadOnlyCollection<IState>> trans4 = new Dictionary<char, IReadOnlyCollection<IState>>();
            Dictionary<char, IReadOnlyCollection<IState>> trans5 = new Dictionary<char, IReadOnlyCollection<IState>>();

            IReadOnlyCollection<IState> etrans0 = new HashSet<IState>();
            IReadOnlyCollection<IState> etrans1 = new HashSet<IState> { states[4] };
            IReadOnlyCollection<IState> etrans2 = new HashSet<IState>();
            IReadOnlyCollection<IState> etrans3 = new HashSet<IState>();
            IReadOnlyCollection<IState> etrans4 = new HashSet<IState> { states[5] };
            IReadOnlyCollection<IState> etrans5 = new HashSet<IState>();

            Mock<INfa<char>> nfa = new Mock<INfa<char>>();
            nfa.Setup(foo => foo.StartingState()).Returns(states[0]);
            nfa.Setup(foo => foo.IsAccepting(states[0])).Returns(false);
            nfa.Setup(foo => foo.IsAccepting(states[1])).Returns(false);
            nfa.Setup(foo => foo.IsAccepting(states[2])).Returns(false);
            nfa.Setup(foo => foo.IsAccepting(states[3])).Returns(false);
            nfa.Setup(foo => foo.IsAccepting(states[4])).Returns(true);
            nfa.Setup(foo => foo.IsAccepting(states[5])).Returns(true);

            nfa.Setup(foo => foo.Transitions(states[0])).Returns(trans0);
            nfa.Setup(foo => foo.Transitions(states[1])).Returns(trans1);
            nfa.Setup(foo => foo.Transitions(states[2])).Returns(trans2);
            nfa.Setup(foo => foo.Transitions(states[3])).Returns(trans3);
            nfa.Setup(foo => foo.Transitions(states[4])).Returns(trans4);
            nfa.Setup(foo => foo.Transitions(states[5])).Returns(trans5);

            nfa.Setup(foo => foo.EpsilonTransitions(states[0])).Returns(etrans0);
            nfa.Setup(foo => foo.EpsilonTransitions(states[1])).Returns(etrans1);
            nfa.Setup(foo => foo.EpsilonTransitions(states[2])).Returns(etrans2);
            nfa.Setup(foo => foo.EpsilonTransitions(states[3])).Returns(etrans3);
            nfa.Setup(foo => foo.EpsilonTransitions(states[4])).Returns(etrans4);
            nfa.Setup(foo => foo.EpsilonTransitions(states[5])).Returns(etrans5);

            IDfa<bool, char> dfa = NfaToDfaConverter<char>.Convert(nfa.Object);
            IState s = dfa.StartingState();
            Assert.IsFalse(dfa.Label(s));
            IReadOnlyDictionary<char, IState> t = dfa.Transitions(s);
            Assert.IsFalse(dfa.Label(t['c']));
            Assert.IsFalse(dfa.Label(t['b']));
            Assert.IsTrue(dfa.Label(t['a']));
        }
    }
}
