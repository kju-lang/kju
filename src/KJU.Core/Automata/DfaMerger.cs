namespace KJU.Core.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DfaMerger<TLabel, Symbol>
    {
        public static IDfa<TLabel, Symbol> Merge(
            IReadOnlyDictionary<TLabel, IDfa<bool, Symbol>> dfas,
            Func<IEnumerable<TLabel>, TLabel> conflictSolver)
        {
            return new MergedDfa(
                dfas.Select(x => Tuple.Create(x.Key, x.Value)).ToList(),
                conflictSolver);
        }

        private class MergedDfa : IDfa<TLabel, Symbol>
        {
            private List<Tuple<TLabel, IDfa<bool, Symbol>>> dfas;
            private Func<IEnumerable<TLabel>, TLabel> conflictSolver;
            private ISet<Symbol> allEdges;

            public MergedDfa(List<Tuple<TLabel, IDfa<bool, Symbol>>> dfas, Func<IEnumerable<TLabel>, TLabel> conflictSolver)
            {
                this.dfas = dfas;
                this.conflictSolver = conflictSolver;
                this.allEdges = new HashSet<Symbol>(this.dfas
                    .SelectMany(x => x.Item2.Transitions(x.Item2.StartingState()).Keys));
            }

            IState IDfa<TLabel, Symbol>.StartingState()
            {
                return new ListState<IState>(
                    this.dfas.Select(x => x.Item2.StartingState()).ToList());
            }

            bool IDfa<TLabel, Symbol>.IsStable(IState state)
            {
                // this is implemented only for minimal DFAs
                throw new NotImplementedException();
            }

            TLabel IDfa<TLabel, Symbol>.Label(IState state)
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

            IReadOnlyDictionary<Symbol, IState> IDfa<TLabel, Symbol>.Transitions(IState state)
            {
                var states = (state as ListState<IState>).Value;
                var result = new Dictionary<Symbol, IState>();

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
                    foreach (KeyValuePair<Symbol, IState> p in newStates)
                    {
                        IState newState = p.Value;
                        Symbol edge = p.Key;
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