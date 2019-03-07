namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using KJU.Core;
    using KJU.Core.Automata;
    using KJU.Core.Regex;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class NfaAcceptance
    {
        public static bool Accepts(INfa nfa, string input)
        {
            var cache = new Dictionary<Tuple<IState, string>, bool>();
            return Accepts(nfa, input, nfa.StartingState(), cache);
        }

        private static bool Accepts(INfa nfa, string input, IState state, Dictionary<Tuple<IState, string>, bool> cache)
        {
            if (input == string.Empty && nfa.IsAccepting(state))
            {
                return true;
            }

            var key = Tuple.Create(state, input);
            if (!cache.ContainsKey(key))
            {
                cache[key] = false; // temporarily mark the (state, string) pair as non-accepting to avoid cycles
                bool result = false;

                // check epsilon transitions
                foreach (var nextState in nfa.EpsilonTransitions(state))
                {
                    result = result || Accepts(nfa, input, nextState, cache);
                }

                // check transitions using the first letter of input
                if (input.Length > 0)
                {
                    char firstLetter = input[0];
                    string inputRemaining = input.Substring(1);

                    var transitions = nfa.Transitions(state).GetValueOrDefault(firstLetter);
                    if (transitions != null)
                    {
                        foreach (var nextState in transitions)
                        {
                            result = result || Accepts(nfa, inputRemaining, nextState, cache);
                        }
                    }
                }

                cache[key] = result;
            }

            return cache.GetValueOrDefault(key);
        }
    }
}