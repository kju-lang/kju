namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;

    public class FirstHelper<TLabel>
    {
        public static IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> GetFirstSymbols(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyCollection<DfaAndState<TLabel>> nullables)
        {
            throw new NotImplementedException();
        }
    }
}
