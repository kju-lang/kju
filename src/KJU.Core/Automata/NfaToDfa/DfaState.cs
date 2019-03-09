namespace KJU.Core.Automata.NfaToDfa
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class DfaState : IState
    {
        public bool Equals(IState other)
        {
            return this.Equals(other as object);
        }
    }
}
