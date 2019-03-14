namespace KJU.Tests.Util
{
    using System.Collections.Generic;
    using System.Linq;

    public class MappingEquivalence
    {
        public static bool AreEquivalentCollection<Key, Value>(
            IReadOnlyDictionary<Key, IReadOnlyCollection<Value>> first,
            IReadOnlyDictionary<Key, IReadOnlyCollection<Value>> second)
        {
            bool result = true;
            result = result && first.Keys.ToHashSet().SetEquals(second.Keys.ToHashSet());
            result = result && first.Keys.All(k => first[k].ToHashSet().SetEquals(second[k].ToHashSet()));

            return result;
        }

        public static bool AreEquivalent<Key, Value>(
            IReadOnlyDictionary<Key, Value> first,
            IReadOnlyDictionary<Key, Value> second)
        {
            bool result = true;
            result = result && first.Keys.ToHashSet().SetEquals(second.Keys.ToHashSet());
            result = result && first.Keys.All(k => first[k].Equals(second[k]));

            return result;
        }
    }
}