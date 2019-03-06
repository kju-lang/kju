namespace KJU.Core.Automata
{
    using System.Collections.Generic;

    public interface INfa
    {
        IState StartingState();

        IReadOnlyDictionary<char, IReadOnlyCollection<IState>> Transitions(IState state);

        IReadOnlyCollection<IState> EpsilonTransitions(IState state);

        bool IsAccepting(IState state);
    }
}
