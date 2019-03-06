namespace KJU.Core.Automata
{
    using System.Collections.Generic;

    // TODO it should be public class IDfa<TLabel> where TLabel : IEquatable<TLabel>
    // but Enum doesnt work with IEquatable
    public interface IDfa<TLabel>
    {
        DfaState StaringState();

        IReadOnlyDictionary<char, DfaState> Transitions(DfaState state);

        TLabel Label(DfaState state);

        // Wherever we move from state we always accept the same label
        bool IsStable(DfaState state);
    }
}
