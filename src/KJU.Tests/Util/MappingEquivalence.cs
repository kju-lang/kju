namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class MappingEquivalence
    {
        public static void AssertAreEquivalentCollection<Key, Value>(
            IReadOnlyDictionary<Key, IReadOnlyCollection<Value>> expected,
            IReadOnlyDictionary<Key, IReadOnlyCollection<Value>> actual,
            string message = null)
        {
            if (!expected.Keys.ToHashSet().SetEquals(actual.Keys.ToHashSet()))
            {
                var assertMessageList = new List<string> { "Key sets are not the same." };
                var excessLeft = expected.Keys.Where(x => !actual.Keys.Contains(x)).ToList();
                var excessRight = actual.Keys.Where(x => !expected.Keys.Contains(x)).ToList();
                if (excessLeft.Count != 0)
                {
                    assertMessageList.Add($"Was expected but not present: {string.Join(", ", excessLeft)}");
                }

                if (excessRight.Count != 0)
                {
                    assertMessageList.Add($"Was present but not expected: {string.Join(", ", excessRight)}");
                }

                if (message != null)
                {
                    assertMessageList.Add(message);
                }

                var assertMessage = string.Join(Environment.NewLine, assertMessageList);
                throw new AssertFailedException(assertMessage);
            }

            if (!expected.Keys.All(k => expected[k].ToHashSet().SetEquals(actual[k].ToHashSet())))
            {
                var problematicKeys = expected.Keys.Where(k => expected[k].ToHashSet().SetEquals(actual[k].ToHashSet()))
                    .ToList();
                var assertMessageList = new List<string> { "Key values for some key(s) are different." };
                problematicKeys.ForEach(key =>
                {
                    var excessLeft = expected[key].Where(x => !actual[key].Contains(x)).ToList();
                    var excessRight = actual[key].Where(x => !expected[key].Contains(x)).ToList();
                    if (excessLeft.Count != 0)
                    {
                        assertMessageList.Add($"For key: {key}, was expected but not present: {string.Join(", ", excessLeft)}");
                    }

                    if (excessRight.Count != 0)
                    {
                        assertMessageList.Add($"For key: {key}, was present but not expected: {string.Join(", ", excessRight)}");
                    }
                });
                if (message != null)
                {
                    assertMessageList.Add(message);
                }

                var assertMessage = string.Join(Environment.NewLine, assertMessageList);
                throw new AssertFailedException(assertMessage);
            }
        }

        public static bool AssertAreEquivalent<Key, Value>(
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