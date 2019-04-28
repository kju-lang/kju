namespace KJU.Core.Util
{
    using System.Collections.Generic;
    using System.Linq;

    public class GraphReverser
    {
        public static IReadOnlyDictionary<T, IReadOnlyCollection<T>> ReverseGraph<T>(
            IReadOnlyDictionary<T, IReadOnlyCollection<T>> graph)
        {
            var reversedRelation = new Dictionary<T, HashSet<T>>();

            graph.Keys.ToList().ForEach(elem => reversedRelation.Add(elem, new HashSet<T>()));
            graph.ToList().ForEach(elem =>
                elem.Value.ToList().ForEach(buddy =>
                    reversedRelation[buddy].Add(elem.Key)));

            return reversedRelation
                .ToDictionary(elem => elem.Key, elem => (IReadOnlyCollection<T>)elem.Value);
        }
    }
}