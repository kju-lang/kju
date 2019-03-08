namespace KJU.Tests.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core;
    using KJU.Core.Automata;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DfaMinimizerTests
    {
        [TestMethod]
        public void SmallExamples()
        {
            // Already minimal
            string description1 = @"
                3 6
                
                1 0 1

                1 2 a
                1 3 b
                2 2 a
                2 2 b
                3 3 a
                3 3 b
            ";
            this.CheckMinimization(this.DfaFromDescription(description1), 2);

            // Can be shrinked to 3 vertices
            string description2 = @"
                6 6
                
                1 2 3 1 2 3

                1 2 a
                2 3 a
                3 4 a
                4 5 a
                5 6 a
                6 1 a
            ";
            this.CheckMinimization(this.DfaFromDescription(description2), 3);

            // 1 vertex is enough :)
            string description3 = @"
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
            this.CheckMinimization(this.DfaFromDescription(description3), 1);

            // Wikipedia example
            string description4 = @"
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
            this.CheckMinimization(this.DfaFromDescription(description4), 3);
        }

        [TestMethod]
        public void BigTests()
        {
            Random rng = new Random(44747);
            for (int tests = 0; tests < 3; ++tests)
            {
                int numberOfLabels = tests + 1;
                int alfabet = 5;
                int n = 100 + rng.Next(100);

                List<int> labels = new List<int>();
                for (int i = 0; i < n; ++i)
                {
                    labels.Add(rng.Next(numberOfLabels));
                }

                var dfa = new DfaTest<int>(labels);
                for (int i = 0; i < n; ++i)
                {
                    for (int j = 0; j < alfabet; ++j)
                    {
                        int to = rng.Next(n);
                        dfa.AddTransition(i, to, (char)j);
                    }
                }

                this.CheckMinimization(dfa);
            }
        }

        private DfaTest<int> DfaFromDescription(string graphDescription)
        {
            int iter = 0;
            var input = graphDescription.Split(null).Where(t => t.Length > 0).ToList();

            int n = int.Parse(input[iter++]);
            int m = int.Parse(input[iter++]);

            var labels = new List<int>();
            for (int i = 0; i < n; ++i)
            {
                int label = int.Parse(input[iter++]);
                labels.Add(label);
            }

            var dfa = new DfaTest<int>(labels);

            for (int i = 0; i < m; ++i)
            {
                int x = int.Parse(input[iter++]) - 1;
                int y = int.Parse(input[iter++]) - 1;
                char c = char.Parse(input[iter++]);
                dfa.AddTransition(x, y, c);
            }

            return dfa;
        }

        private void CheckMinimization<TLabel>(IDfa<TLabel> dfa, int expectedNumberOfStates = -1)
        {
            var minimalDfa = DfaMinimizer<TLabel>.Minimize(dfa);
            int numberOfStates = this.ReachableStates(minimalDfa).Count;
            if (expectedNumberOfStates != -1)
            {
                Assert.AreEqual(expectedNumberOfStates, numberOfStates);
            }

            this.CheckAutomatonEquivalence(dfa, minimalDfa);
            this.CheckStateStability(minimalDfa);
        }

        private void CheckAutomatonEquivalence<TLabel>(IDfa<TLabel> firstDfa, IDfa<TLabel> secondDfa)
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

                Assert.AreEqual(firstStateTransitions.Count, secondStateTransitions.Count, "States have different sets of transitions");

                foreach (var(c, firstNextState) in firstStateTransitions)
                {
                    Assert.IsTrue(secondStateTransitions.ContainsKey(c), $"States have different sets of transitions, {secondStateTransitions.Count} : {string.Join(",", firstStateTransitions.Select((x, y) => ((int)x.Key).ToString()).ToList())}");

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

        private void CheckStateStability<TLabel>(IDfa<TLabel> dfa)
        {
            foreach (var state in this.ReachableStates<TLabel>(dfa))
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

                Assert.AreEqual(dfa.IsStable(state), expectedStability, "Stability of the state is incorrect");
            }
        }

        private HashSet<IState> ReachableStates<TLabel>(IDfa<TLabel> dfa)
        {
            var reachedStates = new HashSet<IState>();

            var queue = new Queue<IState>();
            queue.Enqueue(dfa.StartingState());

            while (queue.Count > 0)
            {
                IState state = queue.Dequeue();
                foreach (KeyValuePair<char, IState> transition in dfa.Transitions(state))
                {
                    if (!reachedStates.Contains(transition.Value))
                    {
                        reachedStates.Add(transition.Value);
                        queue.Enqueue(transition.Value);
                    }
                }
            }

            return reachedStates;
        }

        private class DfaTest<TLabel> : IDfa<TLabel>
        {
            private Dictionary<IState, TLabel> label = new Dictionary<IState, TLabel>();
            private List<DfaTestState> state = new List<DfaTestState>();
            private Dictionary<IState, Dictionary<char, IState>> transitions = new Dictionary<IState, Dictionary<char, IState>>();

            public DfaTest(List<TLabel> labels)
            {
                for (int i = 0; i < labels.Count; ++i)
                {
                    var curState = new DfaTestState(i);
                    this.state.Add(curState);
                    this.label[curState] = labels[i];
                    this.transitions[curState] = new Dictionary<char, IState>();
                }
            }

            public void AddTransition(int x, int y, char c)
            {
                this.transitions[this.state[x]][c] = this.state[y];
            }

            public IState StartingState()
            {
                return this.state[0];
            }

            public IReadOnlyDictionary<char, IState> Transitions(IState state)
            {
                return this.transitions[state];
            }

            public TLabel Label(IState state)
            {
                return this.label[state];
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
                    if (other == null || this.GetType() != other.GetType())
                    {
                        return false;
                    }

                    return this.Id == ((DfaTestState)other).Id;
                }

                public override int GetHashCode()
                {
                    return this.Id.GetHashCode();
                }
            }
        }
    }
}