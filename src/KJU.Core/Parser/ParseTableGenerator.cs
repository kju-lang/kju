namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;
    using KJU.Core.Util;

    public class ParseTableGenerator<TLabel>
    where TLabel : Enum
    {
        public static IReadOnlyDictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>, ParseAction<TLabel>> Parse(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> follow,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> firstPlus)
        {
            var parseTable = new Dictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>, ParseAction<TLabel>>();
            var allSymbols = (TLabel[])Enum.GetValues(typeof(TLabel));

            foreach (var dfa in grammar.Rules.Values)
            {
                var allStates = dfa.GetAllStates();

                foreach (var state in allStates)
                {
                    foreach (var firstSymbol in allSymbols)
                    {
                        var key = Tuple.Create(dfa, state, firstSymbol);
                        var actions = GetAllParseActions(grammar, state, dfa, firstSymbol, follow, firstPlus);

                        if (actions.Count > 1)
                        {
                            throw new InvalidOperationException(
                                $"Many possible actions for state {state} and label {firstSymbol}:{string.Join("; ", actions)}");
                        }
                        else if (actions.Count == 1)
                        {
                            parseTable[key] = actions.ElementAt(0);
                        }
                    }
                }
            }

            return parseTable;
        }

        private static IReadOnlyCollection<ParseAction<TLabel>> GetAllParseActions(
            CompiledGrammar<TLabel> grammar,
            IState state,
            IDfa<Optional<Rule<TLabel>>, TLabel> dfa,
            TLabel label,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> follow,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> firstPlus)
        {
            var actions = new List<ParseAction<TLabel>>();

            var stateLabel = dfa.Label(state);
            var dfaAndState = new DfaAndState<TLabel>() { Dfa = dfa, State = state };
            if (stateLabel.IsSome() && follow.ContainsKey(label) && follow[label].Contains(dfaAndState))
            {
                actions.Add(
                    new ParseAction<TLabel>() { Kind = ParseAction<TLabel>.ActionKind.Reduce, Label = stateLabel.Get().Lhs });
            }

            var transitions = dfa.Transitions(state);
            if (transitions.ContainsKey(label))
            {
                var nextState = transitions[label];
                if (!dfa.IsStable(nextState))
                {
                    actions.Add(
                        new ParseAction<TLabel>() { Kind = ParseAction<TLabel>.ActionKind.Shift, Label = label });
                }
            }

            if (firstPlus.ContainsKey(label))
            {
                var firstPlusStates = firstPlus[label];
                foreach (var transitionLabel in transitions.Keys)
                {
                    var nextState = transitions[transitionLabel];
                    if (!dfa.IsStable(nextState) && grammar.Rules.ContainsKey(transitionLabel))
                    {
                        var subDfa = grammar.Rules[transitionLabel];
                        var subDfaAndStartState = new DfaAndState<TLabel>() { Dfa = subDfa, State = subDfa.StartingState() };

                        if (firstPlusStates.Contains(subDfaAndStartState))
                        {
                            actions.Add(
                                new ParseAction<TLabel>() { Kind = ParseAction<TLabel>.ActionKind.Call, Label = transitionLabel });
                        }
                    }
                }
            }

            return actions;
        }
    }
}