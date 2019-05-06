namespace KJU.Core.Util
{
    using System.Collections.Generic;
    using System.Linq;

    public static class GraphUtils
    {
        public static IReadOnlyDictionary<T, IReadOnlyCollection<T>> TransitiveClosure<T>(
            this IReadOnlyDictionary<T, IReadOnlyCollection<T>> relation)
        {
            var result = relation.ToDictionary(p => p.Key, p => new HashSet<T>(p.Value));
            while (MakeTransitiveClosureStep(relation, result))
            {
            }

            return result.ToDictionary(x => x.Key, x => (IReadOnlyCollection<T>)x.Value);
        }

        private static bool MakeTransitiveClosureStep<T>(IReadOnlyDictionary<T, IReadOnlyCollection<T>> relation, IReadOnlyDictionary<T, HashSet<T>> result)
        {
            var changeOccured = false;
            foreach (var kvp in relation.Keys)
            {
                var sizePreUnion = result[kvp].Count;
                result[kvp].ToList().ForEach(called => result[kvp].UnionWith(relation[called]));
                if (sizePreUnion != result[kvp].Count)
                {
                    changeOccured = true;
                }
            }

            return changeOccured;
        }
    }
}