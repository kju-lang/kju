namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Util;

    public class FirstHelper<Symbol>
    {
        public static IReadOnlyDictionary<DfaAndState<Symbol>, IReadOnlyCollection<Symbol>> GetFirstSymbols(
            CompiledGrammar<Symbol> grammar,
            IReadOnlyCollection<DfaAndState<Symbol>> nullables)
        {
            var nullablesSet = new HashSet<DfaAndState<Symbol>>(nullables);

            var firstSymbols = new Dictionary<DfaAndState<Symbol>, HashSet<Symbol>>();
            var graph = new Dictionary<DfaAndState<Symbol>, List<DfaAndState<Symbol>>>();

            foreach (var rule in grammar.Rules)
            {
                var dfa = rule.Value;
                foreach (var state in DfaUtils.GetAllStates(dfa))
                {
                    var stateEntity = StateEntity(dfa, state);
                    firstSymbols[stateEntity] = new HashSet<Symbol>();
                    graph[stateEntity] = new List<DfaAndState<Symbol>>();

                    foreach (var transition in dfa.Transitions(state))
                    {
                        Symbol symbol = transition.Key;
                        var nextState = transition.Value;
                        if (dfa.Label(nextState).IsNone() && dfa.IsStable(nextState))
                        {
                            continue;
                        }

                        firstSymbols[stateEntity].Add(symbol);

                        if (!grammar.Rules.ContainsKey(symbol))
                        {
                            continue;
                        }

                        var symbolEntity = StateEntity(grammar.Rules[symbol], grammar.Rules[symbol].StartingState());
                        graph[stateEntity].Add(symbolEntity);

                        if (nullablesSet.Contains(symbolEntity))
                        {
                            graph[stateEntity].Add(StateEntity(dfa, transition.Value));
                        }
                    }
                }
            }

            TransitiveClosure(graph, firstSymbols);

            return firstSymbols.ToDictionary(kpv => kpv.Key, kpv => kpv.Value as IReadOnlyCollection<Symbol>);
        }

        private static DfaAndState<Symbol> StateEntity(IDfa<Optional<Rule<Symbol>>, Symbol> dfa, IState state)
        {
            return new DfaAndState<Symbol> { Dfa = dfa, State = state };
        }

        private static void TransitiveClosure(
            Dictionary<DfaAndState<Symbol>, List<DfaAndState<Symbol>>> graph,
            Dictionary<DfaAndState<Symbol>, HashSet<Symbol>> firstSymbols)
        {
            foreach (var initialStateEntity in graph.Select(kvp => kvp.Key))
            {
                var queue = new Queue<DfaAndState<Symbol>>();
                var visited = new HashSet<DfaAndState<Symbol>>();

                queue.Enqueue(initialStateEntity);
                visited.Add(initialStateEntity);

                while (queue.Count > 0)
                {
                    var stateEntity = queue.Dequeue();
                    firstSymbols[initialStateEntity].UnionWith(firstSymbols[stateEntity]);

                    foreach (var nextEntity in graph[stateEntity])
                    {
                        if (!visited.Contains(nextEntity))
                        {
                            queue.Enqueue(nextEntity);
                            visited.Add(nextEntity);
                        }
                    }
                }
            }
        }
    }
}
