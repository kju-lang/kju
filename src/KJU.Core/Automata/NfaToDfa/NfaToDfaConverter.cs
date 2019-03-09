namespace KJU.Core.Automata.NfaToDfa
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public static class NfaToDfaConverter
    {
        public static IDfa<bool> Convert(INfa nfa)
        {
            HashSet<char> alphabet = GetAlphabet(nfa);
            Dictionary<Util.HashableHashSet<IState>, DfaState> map = new Dictionary<Util.HashableHashSet<IState>, DfaState>();
            HashSet<DfaState> accepting = new HashSet<DfaState>();
            Dictionary<DfaState, Dictionary<char, IState>> trans = new Dictionary<DfaState, Dictionary<char, IState>>();
            Queue<Util.HashableHashSet<IState>> q = new Queue<Util.HashableHashSet<IState>>();
            Util.HashableHashSet<IState> start = new Util.HashableHashSet<IState> { nfa.StartingState() };
            Util.HashableHashSet<IState> startingClosure = EpsilonClosure(nfa, start);
            map.Add(startingClosure, new DfaState());
            q.Enqueue(startingClosure);
            while (q.Count != 0)
            {
                Util.HashableHashSet<IState> currentSet = q.Dequeue();
                foreach (IState state in currentSet)
                {
                    if (nfa.IsAccepting(state))
                    {
                        accepting.Add(map[currentSet]);
                        break;
                    }
                }

                Dictionary<char, Util.HashableHashSet<IState>> dict = new Dictionary<char, Util.HashableHashSet<IState>>();
                foreach (IState state in currentSet)
                {
                    IReadOnlyDictionary<char, IReadOnlyCollection<IState>> edges = nfa.Transitions(state);
                    foreach (char key in edges.Keys)
                    {
                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, new Util.HashableHashSet<IState>());
                        }

                        foreach (IState s in edges[key])
                        {
                            dict[key].Add(s);
                        }
                    }
                }

                foreach (char key in alphabet)
                {
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, new Util.HashableHashSet<IState>());
                    }
                }

                trans.Add(map[currentSet], new Dictionary<char, IState>());
                foreach (char key in dict.Keys)
                {
                    Util.HashableHashSet<IState> neighbour = EpsilonClosure(nfa, dict[key]);
                    if (!map.ContainsKey(neighbour))
                    {
                        map.Add(neighbour, new DfaState());

                        q.Enqueue(neighbour);
                    }

                    trans[map[currentSet]].Add(key, map[neighbour]);
                }
            }

            return new Dfa(map[start], accepting, trans);
        }

        private static HashSet<char> GetAlphabet(INfa nfa)
        {
            HashSet<char> alphabet = new HashSet<char>();
            Queue<IState> q = new Queue<IState>();
            IState state = nfa.StartingState();
            Util.HashableHashSet<IState> visited = new Util.HashableHashSet<IState>();
            q.Enqueue(state);
            visited.Add(state);
            while (q.Count != 0)
            {
                state = q.Dequeue();
                IReadOnlyDictionary<char, IReadOnlyCollection<IState>> edges = nfa.Transitions(state);
                foreach (char key in edges.Keys)
                {
                    alphabet.Add(key);
                    foreach (IState neighbour in edges[key])
                    {
                        if (!visited.Contains(neighbour))
                        {
                            visited.Add(neighbour);
                            q.Enqueue(neighbour);
                        }
                    }
                }

                foreach (IState neighbour in nfa.EpsilonTransitions(state))
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        q.Enqueue(neighbour);
                    }
                }
            }

            return alphabet;
        }

        private static Util.HashableHashSet<IState> EpsilonClosure(INfa nfa, Util.HashableHashSet<IState> states)
        {
            Util.HashableHashSet<IState> closure = new Util.HashableHashSet<IState>();
            Queue<IState> q = new Queue<IState>();
            foreach (IState state in states)
            {
                closure.Add(state);
                q.Enqueue(state);
            }

            while (q.Count != 0)
            {
                IState state = q.Dequeue();
                foreach (IState neighbour in nfa.EpsilonTransitions(state))
                {
                    if (!closure.Contains(neighbour))
                    {
                        closure.Add(neighbour);
                        q.Enqueue(neighbour);
                    }
                }
            }

            return closure;
        }
    }
}
