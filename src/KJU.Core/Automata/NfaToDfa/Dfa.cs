namespace KJU.Core.Automata.NfaToDfa
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Dfa<Symbol> : IDfa<bool, Symbol>
    {
        private readonly IState start;
        private readonly HashSet<IState> accepting;
        private readonly Dictionary<DfaState, Dictionary<Symbol, IState>> trans;

        public Dfa(DfaState start, HashSet<DfaState> accepting, Dictionary<DfaState, Dictionary<Symbol, IState>> trans)
        {
            this.accepting = new HashSet<IState>(accepting);
            this.start = start;
            this.trans = trans;
        }

        public bool IsStable(IState state)
        {
            // minimized DFA should implement this
            throw new NotImplementedException("Only Minimized DFA implements IsStable method");
        }

        public bool Label(IState state)
        {
            return this.accepting.Contains(state);
        }

        public IState StartingState()
        {
            return this.start;
        }

        public IReadOnlyDictionary<Symbol, IState> Transitions(IState state)
        {
            if (!(state is DfaState) || !this.trans.ContainsKey((DfaState)state))
            {
                throw new ArgumentException("Passed argument is not a state of this DFA");
            }

            return this.trans[(DfaState)state];
        }
    }
}
