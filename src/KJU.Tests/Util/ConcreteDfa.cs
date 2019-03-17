namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Automata;

    public class ConcreteDfa<TLabel, Symbol> : IDfa<TLabel, Symbol>
    {
        public IDictionary<int, TLabel> Labels { get; } = new Dictionary<int, TLabel>();

        public IDictionary<int, Dictionary<Symbol, int>> Edges { get; } = new Dictionary<int, Dictionary<Symbol, int>>();

        public HashSet<int> StableStates { get; } = new HashSet<int>();

        public int Magic { get; set; } // magic number for distinguishing automata in tests.

        public void AddEdge(int source, Symbol edge, int destination)
        {
            if (!this.Edges.ContainsKey(source))
            {
                this.Edges[source] = new Dictionary<Symbol, int>();
            }

            this.Edges[source][edge] = destination;
        }

        bool IDfa<TLabel, Symbol>.IsStable(IState state)
        {
            return this.StableStates.Contains((state as ValueState<int>).Value);
        }

        TLabel IDfa<TLabel, Symbol>.Label(IState state)
        {
            return this.Labels[(state as ValueState<int>).Value];
        }

        IState IDfa<TLabel, Symbol>.StartingState()
        {
            return new ValueState<int>(0);
        }

        IReadOnlyDictionary<Symbol, IState> IDfa<TLabel, Symbol>.Transitions(IState state)
        {
            var i = (state as ValueState<int>).Value;
            if (!this.Edges.ContainsKey(i))
            {
                return new Dictionary<Symbol, IState>();
            }

            return this.Edges[i]
                .ToDictionary(kv => kv.Key, kv => new ValueState<int>(kv.Value) as IState);
        }
    }
}