namespace KJU.Core.Automata
{
    using System.Collections.Generic;

    public interface INfa<Symbol>
    {
        IState StartingState();

        IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> Transitions(IState state);

        IReadOnlyCollection<IState> EpsilonTransitions(IState state);

        bool IsAccepting(IState state);
    }
}
