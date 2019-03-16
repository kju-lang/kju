namespace KJU.Core.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Inverses a relation R \subset A x B
    /// Be aware: it removes duplicates!
    /// </summary>
    public static class InverseRelationHelper
    {
        public static IReadOnlyDictionary<B, IReadOnlyCollection<A>> InverseRelation<A, B>(
            this IReadOnlyDictionary<A, IReadOnlyCollection<B>> relation)
        {
            var inverseRelation = new Dictionary<B, HashSet<A>>();

            foreach (var relationPairs in relation)
            {
                var firstElementOfPair = relationPairs.Key;
                foreach (var secondElementOfPair in relationPairs.Value)
                {
                    if (!inverseRelation.ContainsKey(secondElementOfPair))
                    {
                        inverseRelation[secondElementOfPair] = new HashSet<A>();
                    }

                    inverseRelation[secondElementOfPair].Add(firstElementOfPair);
                }
            }

            return inverseRelation.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IReadOnlyCollection<A>);
        }
    }
}