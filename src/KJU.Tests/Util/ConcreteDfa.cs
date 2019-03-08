namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Automata;

    public class ConcreteDfa<TLabel> : IDfa<TLabel>
    {
        public IDictionary<int, TLabel> Labels { get; } = new Dictionary<int, TLabel>();

        public IDictionary<int, Dictionary<char, int>> Edges { get; } = new Dictionary<int, Dictionary<char, int>>();

        public void AddEdge(int source, char edge, int destination)
        {
            if (!this.Edges.ContainsKey(source))
            {
                this.Edges[source] = new Dictionary<char, int>();
            }

            this.Edges[source][edge] = destination;
        }

        bool IDfa<TLabel>.IsStable(IState state)
        {
            throw new NotImplementedException();
        }

        TLabel IDfa<TLabel>.Label(IState state)
        {
            return this.Labels[(state as ValueState<int>).Value];
        }

        IState IDfa<TLabel>.StartingState()
        {
            return new ValueState<int>(0);
        }

        IReadOnlyDictionary<char, IState> IDfa<TLabel>.Transitions(IState state)
        {
            var i = (state as ValueState<int>).Value;
            if (!this.Edges.ContainsKey(i))
            {
                return new Dictionary<char, IState>();
            }

            return this.Edges[i]
                .ToDictionary(kv => kv.Key, kv => new ValueState<int>(kv.Value) as IState);
        }
    }
}