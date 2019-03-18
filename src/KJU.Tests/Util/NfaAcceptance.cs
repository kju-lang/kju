namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;

    public static class NfaAcceptance
    {
        public static bool Accepts(this INfa<char> nfa, string input)
        {
            var cache = new Dictionary<Tuple<IState, string>, bool>();
            return Accepts(nfa, input, nfa.StartingState(), cache);
        }

        private static bool Accepts(this INfa<char> nfa, string input, IState state, Dictionary<Tuple<IState, string>, bool> cache)
        {
            if (input == string.Empty && nfa.IsAccepting(state))
            {
                return true;
            }

            var key = Tuple.Create(state, input);
            if (!cache.ContainsKey(key))
            {
                cache[key] = false; // temporarily mark the (state, string) pair as non-accepting to avoid cycles

                // check epsilon transitions
                var result = nfa.EpsilonTransitions(state).Any(nextState => nfa.Accepts(input, nextState, cache));

                // check transitions using the first letter of input
                if (input.Length > 0)
                {
                    var firstLetter = input[0];
                    var remainingInput = input.Substring(1);

                    var transitions = nfa.Transitions(state).GetValueOrDefault(firstLetter);
                    if (transitions != null)
                    {
                        result = result || transitions.Any(nextState => nfa.Accepts(remainingInput, nextState, cache));
                    }
                }

                cache[key] = result;
            }

            return cache.GetValueOrDefault(key);
        }
    }
}