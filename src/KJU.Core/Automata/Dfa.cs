namespace KJU.Core.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    // TODO it should be public class Dfa<TLabel> where TLabel : IEquatable<TLabel>
    // but Enum doesnt work with IEquatable
    public class Dfa<TLabel>
    {
        public DfaState StaringState()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<char, DfaState> Transitions(DfaState state)
        {
            throw new NotImplementedException();
        }

        public TLabel Label(DfaState state)
        {
            throw new NotImplementedException();
        }

        // Wherever we move from state we always accept the same label
        public bool IsStable(DfaState state)
        {
            throw new NotImplementedException();
        }
    }
}
