namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;

    public class FirstPlusHelper<TLabel>
    {
        public static IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> GetFirstPlusSymbols(
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> first,
            IReadOnlyDictionary<TLabel, IReadOnlyCollection<DfaAndState<TLabel>>> follow,
            IReadOnlyCollection<DfaAndState<TLabel>> nullables)
        {
            var firstPlus = first.ToDictionary(e => e.Key, e => new HashSet<DfaAndState<TLabel>>(e.Value));

            foreach (var key in follow.Keys)
            {
                var toAdd = follow[key].Where(s => nullables.Contains(s));
                if (toAdd.Count() > 0)
                {
                    if (!firstPlus.ContainsKey(key))
                    {
                        firstPlus[key] = new HashSet<DfaAndState<TLabel>>();
                    }

                    firstPlus[key].UnionWith(toAdd);
                }
            }

            return firstPlus.ToDictionary(
                e => e.Key, e => (IReadOnlyCollection<DfaAndState<TLabel>>)e.Value);
        }
    }
}