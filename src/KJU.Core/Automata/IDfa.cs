﻿namespace KJU.Core.Automata
{
    using System.Collections.Generic;

    // TODO it should be public class IDfa<TLabel> where TLabel : IEquatable<TLabel>
    // but Enum doesnt work with IEquatable
    public interface IDfa<TLabel, Symbol>
    {
        IState StartingState();

        IState Transition(IState state, Symbol symbol);

        IReadOnlyDictionary<Symbol, IState> Transitions(IState state);

        TLabel Label(IState state);

        // Wherever we move from state we always accept the same label
        bool IsStable(IState state);
    }
}
