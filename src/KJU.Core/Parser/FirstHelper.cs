namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;

    public class FirstHelper<TLabel>
    {
        public static IReadOnlyDictionary<TLabel, IReadOnlyCollection<Tuple<IDfa<TLabel>, IState>>> GetFirstSymbols(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyCollection<Tuple<IDfa<TLabel>, IState>> nullables)
        {
            throw new NotImplementedException();
        }
    }
}
