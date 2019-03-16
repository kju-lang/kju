namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Util;

    public class NullablesHelper<TLabel>
    {
        public static IReadOnlyCollection<DfaAndState<TLabel>> GetNullableSymbols(CompiledGrammar<TLabel> grammar)
        {
            // TLabel - nieterminal
            // Rule<TLabel> - produkcja
            var statesToProcess = new Queue<DfaAndState<TLabel>>();
            var dfaToLhSymbol = new Dictionary<IDfa<Optional<Rule<TLabel>>, TLabel>, TLabel>();
            var nullableSymbols = new HashSet<TLabel>();
            var potentialNullableStates = new Dictionary<TLabel, List<DfaAndState<TLabel>>>();
            var visitedStates = new HashSet<DfaAndState<TLabel>>(); // all visited states are nullable

            foreach (var rule in grammar.Rules)
            {
                var ruleDfa = rule.Value;
                foreach (var state in ruleDfa.GetAllStates())
                {
                    if (IsAccepting(ruleDfa, state))
                    {
                        var queuePair = CreateDfaAndState(ruleDfa, state);
                        if (!visitedStates.Contains(queuePair))
                        {
                            visitedStates.Add(queuePair);
                            statesToProcess.Enqueue(queuePair);
                        }

                        if (!dfaToLhSymbol.ContainsKey(ruleDfa))
                        {
                            dfaToLhSymbol.Add(ruleDfa, rule.Key);
                        }
                    }
                }
            }

            var reverseTransitions = GetReverseTransitions(grammar);
            while (statesToProcess.Count > 0)
            {
                var queuePair = statesToProcess.Dequeue();

                if (queuePair.State.Equals(queuePair.Dfa.StartingState()))
                {
                    var lhSymbol = dfaToLhSymbol[queuePair.Dfa];
                    nullableSymbols.Add(lhSymbol);
                    if (!potentialNullableStates.ContainsKey(lhSymbol))
                    {
                        potentialNullableStates.Add(lhSymbol, new List<DfaAndState<TLabel>>());
                    }

                    foreach (var stateDfaPair in potentialNullableStates[lhSymbol])
                    {
                        if (!visitedStates.Contains(stateDfaPair))
                        {
                            visitedStates.Add(stateDfaPair);
                            statesToProcess.Enqueue(stateDfaPair);
                        }
                    }

                    potentialNullableStates.Clear();
                }

                foreach (var transition in reverseTransitions[queuePair.Dfa][queuePair.State])
                {
                    var symbol = transition.Key;
                    foreach (var newState in transition.Value)
                    {
                        var newQueuePair = CreateDfaAndState(queuePair.Dfa, newState);
                        if (visitedStates.Contains(newQueuePair))
                        {
                            continue;
                        }

                        if (nullableSymbols.Contains(symbol))
                        {
                            if (!visitedStates.Contains(newQueuePair))
                            {
                                visitedStates.Add(newQueuePair);
                                statesToProcess.Enqueue(newQueuePair);
                            }
                        }
                        else
                        {
                            if (!potentialNullableStates.ContainsKey(symbol))
                            {
                                potentialNullableStates.Add(symbol, new List<DfaAndState<TLabel>>());
                            }

                            potentialNullableStates[symbol].Add(newQueuePair);
                        }
                    }
                }
            }

            return visitedStates;
        }

        private static DfaAndState<TLabel> CreateDfaAndState(IDfa<Optional<Rule<TLabel>>, TLabel> dfa, IState state)
        {
            var pair = new DfaAndState<TLabel>
            {
                Dfa = dfa,
                State = state
            };
            return pair;
        }

        private static bool IsAccepting(IDfa<Optional<Rule<TLabel>>, TLabel> dfa, IState state)
        {
            return !dfa.Label(state).IsNone();
        }

        private static IReadOnlyDictionary<IDfa<Optional<Rule<TLabel>>, TLabel>, Dictionary<IState, Dictionary<TLabel, HashSet<IState>>>> GetReverseTransitions(CompiledGrammar<TLabel> grammar)
        {
            var reverseTransitions = new Dictionary<IDfa<Optional<Rule<TLabel>>, TLabel>, Dictionary<IState, Dictionary<TLabel, HashSet<IState>>>>();

            foreach (var rule in grammar.Rules)
            {
                reverseTransitions.Add(rule.Value, GetReverseTransitions(rule.Value));
            }

            return reverseTransitions;
        }

        private static Dictionary<IState, Dictionary<TLabel, HashSet<IState>>> GetReverseTransitions(IDfa<Optional<Rule<TLabel>>, TLabel> dfa)
        {
            var allStates = dfa.GetAllStates();
            var reverseTransitions = new Dictionary<IState, Dictionary<TLabel, HashSet<IState>>>();

            foreach (IState state in allStates)
            {
                reverseTransitions.Add(state, new Dictionary<TLabel, HashSet<IState>>());
            }

            foreach (IState state in allStates)
            {
                foreach (var transition in dfa.Transitions(state))
                {
                    var symbol = transition.Key;
                    var newState = transition.Value;
                    if (!reverseTransitions[newState].ContainsKey(symbol))
                    {
                        reverseTransitions[newState].Add(symbol, new HashSet<IState>());
                    }

                    reverseTransitions[transition.Value][transition.Key].Add(state);
                }
            }

            return reverseTransitions;
        }
    }
}
