namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;

    public class FollowHelper<TLabel>
    {
        public static IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> GetFollowSymbols(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyCollection<DfaAndState<TLabel>> nullables,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> first)
        {
            throw new NotImplementedException();
        }
    }
}
