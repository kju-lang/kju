namespace KJU.Tests.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DfaMinimizerTests
    {
        [TestMethod]
        public void AlreadyMinimal()
        {
            var description = @"
                3 6
                
                1 0 1

                1 2 a
                1 3 b
                2 2 a
                2 2 b
                3 3 a
                3 3 b
            ";
            CheckMinimization(DfaFromDescription(description), 2);
        }

        [TestMethod]
        public void CanBeShrinkedToThreeVertices()
        {
            var description = @"
                6 6
                
                1 2 3 1 2 3

                1 2 a
                2 3 a
                3 4 a
                4 5 a
                5 6 a
                6 1 a
            ";
            CheckMinimization(DfaFromDescription(description), 3);
        }

        [TestMethod]
        public void OneVertexIsEnough()
        {
            var description = @"
                6 12
                
                1 1 1 1 1 1

                1 2 a
                1 5 b
                2 3 a
                2 6 b
                3 4 a
                3 1 b
                4 5 a
                4 5 b
                5 6 a
                5 5 b
                6 1 a
                6 6 b
            ";
            CheckMinimization(DfaFromDescription(description), 1);
        }

        [TestMethod]
        public void WikipediaExample()
        {
            var description = @"
                6 12
                
                0 0 1 1 1 0

                1 2 a
                1 3 b
                2 1 a
                2 4 b
                3 5 a
                3 6 b
                4 5 a
                4 6 b
                5 5 a
                5 6 b
                6 6 a
                6 6 b
            ";
            CheckMinimization(DfaFromDescription(description), 3);
        }

        [TestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 2)]
        [DataRow(3, 3)]
        public void BigTests(int seed, int numberOfLabels)
        {
            var rng = new Random(seed);
            var alphabetSize = 5;
            var n = 100 + rng.Next(100);

            var labels = Enumerable.Range(0, n).Select(x => rng.Next(numberOfLabels)).ToList();

            var dfa = new DfaTest<int>(labels);
            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < alphabetSize; ++j)
                {
                    var to = rng.Next(n);
                    dfa.AddTransition(i, to, (char)j);
                }
            }

            CheckMinimization(dfa);
        }

        private static DfaTest<int> DfaFromDescription(string graphDescription)
        {
            var iter = 0;
            var input = graphDescription.Split(null).Where(t => t.Length > 0).ToList();

            var n = int.Parse(input[iter++]);
            var m = int.Parse(input[iter++]);

            var labels = Enumerable.Range(0, n).Select(x => int.Parse(input[iter++])).ToList();

            var dfa = new DfaTest<int>(labels);

            for (var i = 0; i < m; ++i)
            {
                var from = int.Parse(input[iter++]) - 1;
                var to = int.Parse(input[iter++]) - 1;
                var letter = char.Parse(input[iter++]);
                dfa.AddTransition(from, to, letter);
            }

            return dfa;
        }

        private static void CheckMinimization<TLabel>(IDfa<TLabel, char> dfa, int expectedNumberOfStates = -1)
        {
            var minimalDfa = DfaMinimizer<TLabel, char>.Minimize(dfa);
            var numberOfStates = ReachableStates(minimalDfa).Count;
            if (expectedNumberOfStates != -1)
            {
                Assert.AreEqual(expectedNumberOfStates, numberOfStates);
            }

            CheckAutomatonEquivalence(dfa, minimalDfa);
            CheckStateStability(minimalDfa);
        }

        private static void CheckAutomatonEquivalence<TLabel>(IDfa<TLabel, char> firstDfa, IDfa<TLabel, char> secondDfa)
        {
            var reached = new HashSet<Tuple<IState, IState>>();
            var queue = new Queue<Tuple<IState, IState>>();

            queue.Enqueue(Tuple.Create(firstDfa.StartingState(), secondDfa.StartingState()));
            reached.Add(queue.Peek());

            while (queue.Count > 0)
            {
                var(firstState, secondState) = queue.Dequeue();
                Assert.AreEqual(firstDfa.Label(firstState), secondDfa.Label(secondState), "Labels should be equal");
                var firstStateTransitions = firstDfa.Transitions(firstState);
                var secondStateTransitions = secondDfa.Transitions(secondState);

                var firstTransitionCount = firstStateTransitions.Count;
                var secondTransitionCount = secondStateTransitions.Count;
                Assert.AreEqual(
                    firstTransitionCount,
                    secondTransitionCount,
                    $"States {firstState} and {secondState} have different number of transitions: {firstTransitionCount} != {secondTransitionCount}");

                foreach (var(c, firstNextState) in firstStateTransitions)
                {
                    Assert.IsTrue(
                        secondStateTransitions.ContainsKey(c),
                        $"States have different sets of transitions, {secondTransitionCount} : {string.Join(",", firstStateTransitions.Select((x, y) => ((int)x.Key).ToString()).ToList())}");

                    var secondNextState = secondStateTransitions[c];

                    var pair = Tuple.Create(firstNextState, secondNextState);
                    if (!reached.Contains(pair))
                    {
                        reached.Add(pair);
                        queue.Enqueue(pair);
                    }
                }
            }
        }

        private static void CheckStateStability<TLabel>(IDfa<TLabel, char> dfa)
        {
            foreach (var state in ReachableStates(dfa))
            {
                bool expectedStability = true;
                foreach (var(_, nextState) in dfa.Transitions(state))
                {
                    if (nextState != state)
                    {
                        expectedStability = false;
                        break;
                    }
                }

                Assert.AreEqual(expectedStability, dfa.IsStable(state), "Stability of the state is incorrect");
            }
        }

        private static HashSet<IState> ReachableStates<TLabel>(IDfa<TLabel, char> dfa)
        {
            var reachedStates = new HashSet<IState>();

            var queue = new Queue<IState>(new[] { dfa.StartingState() });

            while (queue.Count > 0)
            {
                var state = queue.Dequeue();
                foreach (var(_, value) in dfa.Transitions(state))
                {
                    if (!reachedStates.Contains(value))
                    {
                        reachedStates.Add(value);
                        queue.Enqueue(value);
                    }
                }
            }

            return reachedStates;
        }

        private class DfaTest<TLabel> : IDfa<TLabel, char>
        {
            private readonly Dictionary<IState, TLabel> labels = new Dictionary<IState, TLabel>();
            private readonly List<DfaTestState> states = new List<DfaTestState>();

            private readonly Dictionary<IState, Dictionary<char, IState>> transitions =
                new Dictionary<IState, Dictionary<char, IState>>();

            public DfaTest(List<TLabel> labels)
            {
                for (var i = 0; i < labels.Count; ++i)
                {
                    var curState = new DfaTestState(i);
                    this.states.Add(curState);
                    this.labels[curState] = labels[i];
                    this.transitions[curState] = new Dictionary<char, IState>();
                }
            }

            public void AddTransition(int x, int y, char c)
            {
                this.transitions[this.states[x]][c] = this.states[y];
            }

            public IState StartingState()
            {
                return this.states[0];
            }

            public IReadOnlyDictionary<char, IState> Transitions(IState state)
            {
                return this.transitions[state];
            }

            public TLabel Label(IState state)
            {
                return this.labels[state];
            }

            public bool IsStable(IState state)
            {
                return false;
            }

            private class DfaTestState : IState
            {
                public DfaTestState(int id)
                {
                    this.Id = id;
                }

                public int Id { get; }

                public bool Equals(IState other)
                {
                    if (other is DfaTestState otherState)
                    {
                        return this.Id == otherState.Id;
                    }

                    return false;
                }

                public override int GetHashCode()
                {
                    return this.Id.GetHashCode();
                }
            }
        }
    }
}