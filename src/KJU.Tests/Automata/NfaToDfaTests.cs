namespace KJU.Tests.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Automata.NfaToDfa;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NfaToDfaTests
    {
        [TestMethod]
        public void TestNfaToDfa()
        {
            var nfa = new IntNfa();

            var dfa = NfaToDfaConverter<char>.Convert(nfa);

            var start = dfa.StartingState();
            var startingLabel = dfa.Label(start);
            var startingEdges = dfa.Transitions(start);
            var aState = startingEdges['a'];
            var aLabel = dfa.Label(aState);
            var aEdges = dfa.Transitions(aState);
            Assert.IsFalse(startingLabel);
            Assert.IsTrue(aLabel);
            Assert.AreEqual(1, startingEdges.Count);
            Assert.AreEqual(1, aEdges.Count);
            Assert.AreEqual(startingEdges['a'], aState);
        }

        [TestMethod]
        public void TestNfaToDfaBigger()
        {
            var states = Enumerable.Range(0, 6).Select(x => new IntState(x)).ToList();

            var trans0 = new Dictionary<char, IReadOnlyCollection<IState>>
            {
                ['a'] = new HashSet<IState> { states[1], states[2] },
                ['b'] = new HashSet<IState> { states[2], states[3] }
            };

            var trans1 = new Dictionary<char, IReadOnlyCollection<IState>>
            {
                ['c'] = new HashSet<IState> { states[2] }
            };

            var trans2 = new Dictionary<char, IReadOnlyCollection<IState>>();

            var trans3 = new Dictionary<char, IReadOnlyCollection<IState>>();
            var trans4 = new Dictionary<char, IReadOnlyCollection<IState>>();
            var trans5 = new Dictionary<char, IReadOnlyCollection<IState>>();

            IReadOnlyCollection<IState> etrans0 = new HashSet<IState>();
            IReadOnlyCollection<IState> etrans1 = new HashSet<IState> { states[4] };
            IReadOnlyCollection<IState> etrans2 = new HashSet<IState>();
            IReadOnlyCollection<IState> etrans3 = new HashSet<IState>();
            IReadOnlyCollection<IState> etrans4 = new HashSet<IState> { states[5] };
            IReadOnlyCollection<IState> etrans5 = new HashSet<IState>();

            var nfa = new Mock<INfa<char>>();
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

            var dfa = NfaToDfaConverter<char>.Convert(nfa.Object);
            var startingState = dfa.StartingState();
            var startingTransitions = dfa.Transitions(startingState);
            Assert.IsFalse(dfa.Label(startingState));
            Assert.IsFalse(dfa.Label(startingTransitions['c']));
            Assert.IsFalse(dfa.Label(startingTransitions['b']));
            Assert.IsTrue(dfa.Label(startingTransitions['a']));
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

        private class IntNfa : INfa<char>
        {
            public IReadOnlyCollection<IState> EpsilonTransitions(IState state)
            {
                return new HashSet<IState>(new[] { new IntState(0) });
            }

            public bool IsAccepting(IState state)
            {
                return new IntState(1).Equals(state);
            }

            public IState StartingState()
            {
                return new IntState(0);
            }

            public IReadOnlyDictionary<char, IReadOnlyCollection<IState>> Transitions(IState state)
            {
                if (!(state is IntState))
                {
                    throw new Exception($"Required int state. Got: {state.GetType()}");
                }

                IReadOnlyCollection<IState> col = new HashSet<IState>(new[] { new IntState(0), new IntState(1) });
                return new Dictionary<char, IReadOnlyCollection<IState>> { { 'a', col } };
            }
        }
    }
}
