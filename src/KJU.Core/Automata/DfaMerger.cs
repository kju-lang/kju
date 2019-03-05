namespace KJU.Core.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class DfaMerger<TLabel>
    {
        public static Dfa<TLabel> Merge(
            IReadOnlyDictionary<TLabel, Dfa<bool>> dfas,
            Func<IEnumerable<TLabel>, TLabel> conflictSolver)
        {
            throw new NotImplementedException();
        }
    }
}
