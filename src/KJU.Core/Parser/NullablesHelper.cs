namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;

    public class NullablesHelper<TLabel>
    {
        public static IReadOnlyCollection<Tuple<IDfa<TLabel>, IState>> GetNullableSymbols(CompiledGrammar<TLabel> grammar)
        {
            throw new NotImplementedException();
        }
    }
}
