namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;

    public class FollowHelper<TLabel>
    {
        public static IReadOnlyDictionary<TLabel, IReadOnlyCollection<Tuple<IDfa<TLabel>, IState>>> GetFollowSymbols(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyCollection<Tuple<IDfa<TLabel>, IState>> nullables,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<Tuple<IDfa<TLabel>, IState>>> first)
        {
            throw new NotImplementedException();
        }
    }
}
