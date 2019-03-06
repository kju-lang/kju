namespace KJU.Core.Automata
{
    using System.Collections.Generic;

    public interface INfa
    {
        NfaState StaringState();

        IReadOnlyDictionary<char, IReadOnlyCollection<NfaState>> Transitions(NfaState state);

        IReadOnlyCollection<NfaState> EpsilonTransitions(NfaState state);

        bool IsAccepting(NfaState state);
    }
}
