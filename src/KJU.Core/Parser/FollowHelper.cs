namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;

    public class FollowHelper<TLabel>
    {
        public static IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaState<TLabel>>> GetFollowSymbols(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyCollection<DfaState<TLabel>> nullables,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaState<TLabel>>> first)
        {
            throw new NotImplementedException();
        }
    }
}
