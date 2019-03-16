namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;

    public class FollowHelper<TLabel>
    {
        public static IReadOnlyDictionary<DfaAndState<TLabel>, IReadOnlyCollection<TLabel>> GetFollowSymbols(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyCollection<DfaAndState<TLabel>> nullables,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> first,
            TLabel eofSymbol)
        {
            var resultSymbols = new Dictionary<TLabel, HashSet<TLabel>>();
            GetDefault(resultSymbols, grammar.StartSymbol).Add(eofSymbol);

            var resultStates = new Dictionary<DfaAndState<TLabel>, HashSet<TLabel>>();

            while (true)
            {
                bool anythingChanged = false;

                foreach (var rule in grammar.Rules)
                {
                    var dfa = rule.Value;
                    foreach (var state in DfaUtils.GetAllStates(dfa))
                    {
                        var label = dfa.Label(state);
                        var dfaAndState = new DfaAndState<TLabel> { Dfa = dfa, State = state };
                        var stateFollows = GetDefault(resultStates, dfaAndState);

                        if (label.IsSome())
                        {
                            var symFollows = GetDefault(resultSymbols, label.Get().Lhs);

                            foreach (var sym in symFollows)
                                anythingChanged = stateFollows.Add(sym) || anythingChanged;

                            // foreach (var sym in stateFollows)
                            //     anythingChanged = symFollows.Add(sym) || anythingChanged;
                        }

                        foreach (var transition in dfa.Transitions(state))
                        {
                            TLabel edgeSymbol = transition.Key;
                            var nextState = transition.Value;
                            var nextDfaAndState = new DfaAndState<TLabel> { Dfa = dfa, State = state };
                            if (dfa.Label(nextState).IsNone() && dfa.IsStable(nextState)) continue;

                            var edgeFollows = GetDefault(resultSymbols, edgeSymbol);

                            foreach (var sym in GetDefault(resultStates, nextDfaAndState))
                                anythingChanged = edgeFollows.Add(sym) || anythingChanged;
                        }
                    }
                }

                if (!anythingChanged) break;
            }

            return resultStates.ToDictionary(kpv => kpv.Key, kpv => kpv.Value as IReadOnlyCollection<TLabel>);
        }

        private static HashSet<V> GetDefault<K, V>(Dictionary<K, HashSet<V>> d, K key)
        {
            if (!d.ContainsKey(key))
                d[key] = new HashSet<V>();

            return d[key];
        }
    }
}
