namespace KJU.Core.Automata.NfaToDfa
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public static class NfaToDfaConverter<Symbol>
    {
        public static IDfa<bool, Symbol> Convert(INfa<Symbol> nfa)
        {
            HashSet<Symbol> alphabet = GetAlphabet(nfa);
            Dictionary<Util.HashableHashSet<IState>, DfaState> map = new Dictionary<Util.HashableHashSet<IState>, DfaState>();
            HashSet<DfaState> accepting = new HashSet<DfaState>();
            Dictionary<DfaState, Dictionary<Symbol, IState>> trans = new Dictionary<DfaState, Dictionary<Symbol, IState>>();
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

                Dictionary<Symbol, Util.HashableHashSet<IState>> dict = new Dictionary<Symbol, Util.HashableHashSet<IState>>();
                foreach (IState state in currentSet)
                {
                    IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> edges = nfa.Transitions(state);
                    foreach (Symbol key in edges.Keys)
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

                foreach (Symbol key in alphabet)
                {
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, new Util.HashableHashSet<IState>());
                    }
                }

                trans.Add(map[currentSet], new Dictionary<Symbol, IState>());
                foreach (Symbol key in dict.Keys)
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

            return new Dfa<Symbol>(map[startingClosure], accepting, trans);
        }

        private static HashSet<Symbol> GetAlphabet(INfa<Symbol> nfa)
        {
            HashSet<Symbol> alphabet = new HashSet<Symbol>();
            Queue<IState> q = new Queue<IState>();
            IState state = nfa.StartingState();
            Util.HashableHashSet<IState> visited = new Util.HashableHashSet<IState>();
            q.Enqueue(state);
            visited.Add(state);
            while (q.Count != 0)
            {
                state = q.Dequeue();
                IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> edges = nfa.Transitions(state);
                foreach (Symbol key in edges.Keys)
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

        private static Util.HashableHashSet<IState> EpsilonClosure(INfa<Symbol> nfa, Util.HashableHashSet<IState> states)
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
