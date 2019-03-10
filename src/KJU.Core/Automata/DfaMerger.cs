namespace KJU.Core.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DfaMerger<TLabel>
    {
        public static IDfa<TLabel> Merge(
            IReadOnlyDictionary<TLabel, IDfa<bool>> dfas,
            Func<IEnumerable<TLabel>, TLabel> conflictSolver)
        {
            return new MergedDfa(
                dfas.Select(x => Tuple.Create(x.Key, x.Value)).ToList(),
                conflictSolver);
        }

        private class MergedDfa : IDfa<TLabel>
        {
            private List<Tuple<TLabel, IDfa<bool>>> dfas;
            private Func<IEnumerable<TLabel>, TLabel> conflictSolver;
            private ISet<char> allEdges;

            public MergedDfa(List<Tuple<TLabel, IDfa<bool>>> dfas, Func<IEnumerable<TLabel>, TLabel> conflictSolver)
            {
                this.dfas = dfas;
                this.conflictSolver = conflictSolver;
                this.allEdges = new HashSet<char>(this.dfas
                    .SelectMany(x => x.Item2.Transitions(x.Item2.StartingState()).Keys));
            }

            IState IDfa<TLabel>.StartingState()
            {
                return new ListState<IState>(
                    this.dfas.Select(x => x.Item2.StartingState()).ToList());
            }

            bool IDfa<TLabel>.IsStable(IState state)
            {
                // this is implemented only for minimal DFAs
                throw new NotImplementedException();
            }

            TLabel IDfa<TLabel>.Label(IState state)
            {
                var states = (state as ListState<IState>).Value;
                return this.conflictSolver(states
                        .Select((innerState, index) => new
                        {
                            // in null state, the automata can't ever match
                            isSuccess = innerState != null && this.dfas[index].Item2.Label(innerState),
                            label = this.dfas[index].Item1
                        })
                        .Where((x) => x.isSuccess)
                        .Select((x) => x.label));
            }

            IReadOnlyDictionary<char, IState> IDfa<TLabel>.Transitions(IState state)
            {
                var states = (state as ListState<IState>).Value;
                var result = new Dictionary<char, IState>();

                foreach (var edge in this.allEdges)
                {
                    result[edge] = new ListState<IState>(states.Select(x => null as IState).ToList());
                }

                for (int i = 0; i < states.Count(); i++)
                {
                    if (states[i] == null)
                    {
                        continue;
                    }

                    var newStates = this.dfas[i].Item2.Transitions(states[i]);
                    foreach (KeyValuePair<char, IState> p in newStates)
                    {
                        IState newState = p.Value;
                        char edge = p.Key;
                        if (!result.ContainsKey(edge))
                        {
                            // create empty state (only nulls) - null acts as an implicit fail state
                            result[edge] = new ListState<IState>(states.Select(x => null as IState).ToList());
                        }

                        (result[edge] as ListState<IState>).Value[i] = newState;
                    }
                }

                return result;
            }
        }
    }
}