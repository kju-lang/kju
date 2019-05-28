namespace KJU.Core.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DfaMinimizer<TLabel, Symbol>
    {
        // Be aware! The minimal dfa for the a given one will reuse some states!
        public static IDfa<TLabel, Symbol> Minimize(IDfa<TLabel, Symbol> dfa)
        {
            List<HashSet<IState>> partition = InitialPartition(dfa);
            while (true)
            {
                var newPartition = MooresStep(dfa, partition);
                if (PartitionRepresentation(partition) == PartitionRepresentation(newPartition))
                {
                    break;
                }

                partition = newPartition;
            }

            return new MinimalDfa(dfa, partition);
        }

        private static string PartitionRepresentation(List<HashSet<IState>> partition)
        {
            return string.Join(",", partition.Select(hs => string.Join(" ", hs.Select(i => Convert.ToString(i)))));
        }

        private static List<HashSet<IState>> InitialPartition(IDfa<TLabel, Symbol> dfa)
        {
            var partition = new List<HashSet<IState>>();
            var labelId = new Dictionary<TLabel, int>();
            int labelCount = 0;

            foreach (IState state in ReachableStates(dfa))
            {
                TLabel stateLabel = dfa.Label(state);
                if (!labelId.ContainsKey(stateLabel))
                {
                    labelId.Add(stateLabel, labelCount);
                    partition.Add(new HashSet<IState>());
                    ++labelCount;
                }

                partition[labelId[stateLabel]].Add(state);
            }

            return partition;
        }

        private static HashSet<IState> ReachableStates(IDfa<TLabel, Symbol> dfa)
        {
            var reachedStates = new HashSet<IState>();

            var queue = new Queue<IState>();
            queue.Enqueue(dfa.StartingState());
            reachedStates.Add(dfa.StartingState());

            while (queue.Count > 0)
            {
                IState state = queue.Dequeue();
                foreach (KeyValuePair<Symbol, IState> transition in dfa.Transitions(state))
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

        private static List<HashSet<IState>> MooresStep(IDfa<TLabel, Symbol> dfa, List<HashSet<IState>> partition)
        {
            var stateClassId = new Dictionary<IState, int>();
            for (int i = 0; i < partition.Count; ++i)
            {
                foreach (var state in partition[i])
                {
                    stateClassId[state] = i;
                }
            }

            var stateDerivatives = new List<Tuple<IState, List<int>>>();
            foreach (var equivalenceClass in partition)
            {
                foreach (var state in equivalenceClass)
                {
                    var derivative = new List<int> { stateClassId[state] };
                    derivative.AddRange(dfa.Transitions(state).OrderBy(t => t.Key).Select(t => stateClassId[t.Value]));
                    stateDerivatives.Add(Tuple.Create(state, derivative));
                }
            }

            var newPartition = new List<HashSet<IState>>();

            var groups = stateDerivatives.GroupBy(d => string.Join(" ", d.Item2.Select(t => Convert.ToString(t))));
            foreach (var group in groups)
            {
                var partitionGroup = new HashSet<IState>();
                foreach (var(state, _) in group)
                {
                    partitionGroup.Add(state);
                }

                newPartition.Add(partitionGroup);
            }

            return newPartition;
        }

        public class MinimalDfa : IDfa<TLabel, Symbol>
        {
            private IDfa<TLabel, Symbol> dfa;
            private List<HashSet<IState>> statePartition;
            private Dictionary<IState, IState> stateMapping = new Dictionary<IState, IState>();
            private HashSet<IState> stableStates = new HashSet<IState>();

            public MinimalDfa(IDfa<TLabel, Symbol> dfa, List<HashSet<IState>> statePartition)
            {
                this.dfa = dfa;
                this.statePartition = statePartition;

                this.CreateStateMapping();
                this.FindStableStates();
            }

            public IState StartingState()
            {
                return this.stateMapping[this.dfa.StartingState()];
            }

            public IState Transition(IState state, Symbol symbol)
            {
                var newState = this.dfa.Transition(state, symbol);

                if (newState != null)
                {
                    return this.stateMapping[newState];
                }

                return null;
            }

            public IReadOnlyDictionary<Symbol, IState> Transitions(IState state)
            {
                return this.dfa.Transitions(state).ToDictionary(kvp => kvp.Key, kvp => this.stateMapping[kvp.Value]);
            }

            public TLabel Label(IState state)
            {
                return this.dfa.Label(state);
            }

            public bool IsStable(IState state)
            {
                return this.stableStates.Contains(state);
            }

            private void CreateStateMapping()
            {
                for (int i = 0; i < this.statePartition.Count; ++i)
                {
                    foreach (var state in this.statePartition[i])
                    {
                        this.stateMapping[state] = this.statePartition[i].First();
                    }
                }
            }

            private void FindStableStates()
            {
                for (int i = 0; i < this.statePartition.Count; ++i)
                {
                    bool same = true;
                    foreach (var state in this.statePartition[i])
                    {
                        foreach (var transition in this.Transitions(state))
                        {
                            if (this.statePartition[i].First() != this.stateMapping[transition.Value])
                            {
                                same = false;
                                break;
                            }
                        }

                        if (!same)
                        {
                            break;
                        }
                    }

                    if (same)
                    {
                        this.stableStates.Add(this.statePartition[i].First());
                    }
                }
            }
        }
    }
}
