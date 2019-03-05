namespace KJU.Core.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;

    public class Nfa
    {
        public NfaState StaringState()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<char, IReadOnlyCollection<NfaState>> Transitions(NfaState state)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<NfaState> EpsilonTransitions(NfaState state)
        {
            throw new NotImplementedException();
        }

        public bool IsAccepting(NfaState state)
        {
            throw new NotImplementedException();
        }
    }
}
