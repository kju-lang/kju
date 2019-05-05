namespace KJU.Core.Util
{
    using System.Collections.Generic;
    using System.Linq;

    public static class GraphUtils
    {
        public static IReadOnlyDictionary<T, IReadOnlyCollection<T>> TransitiveClosure<T>(
            this IReadOnlyDictionary<T, IReadOnlyCollection<T>> relation)
        {
            var result = new Dictionary<T, HashSet<T>>();
            relation.ToList().ForEach(p => result.Add(p.Key, new HashSet<T>()));
            relation.ToList().ForEach(p => result[p.Key].UnionWith(relation[p.Key]));
            var changeOccured = true;
            while (changeOccured)
            {
                changeOccured = false;
                foreach (var key in relation.Keys)
                {
                    var sizePreUnion = result[key].Count;
                    result[key].ToList().ForEach(called => result[key].UnionWith(relation[called]));
                    if (sizePreUnion != result[key].Count)
                    {
                        changeOccured = true;
                    }
                }
            }

            var castResult = new Dictionary<T, IReadOnlyCollection<T>>();
            foreach (var p in result)
            {
                castResult.Add(p.Key, p.Value);
            }

            return castResult;
        }
    }
}