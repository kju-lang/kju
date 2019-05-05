namespace KJU.Tests.Util
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransitiveClosureTest
    {
        [TestMethod]
        public void Line()
        {
            var relation = new Dictionary<int, IReadOnlyCollection<int>>()
            {
                [0] = new HashSet<int>() { 1 },
                [1] = new HashSet<int>() { 2 },
                [2] = new HashSet<int>() { 3 },
                [3] = new HashSet<int>() { },
            };
            var expected = new Dictionary<int, IReadOnlyCollection<int>>()
            {
                [0] = new HashSet<int>() { 1, 2, 3 },
                [1] = new HashSet<int>() { 2, 3 },
                [2] = new HashSet<int>() { 3 },
                [3] = new HashSet<int>() { },
            };

            var closure = relation.TransitiveClosure();

            Assert.IsTrue(closure.Keys.ToHashSet().SetEquals(expected.Keys.ToHashSet()));
            foreach (var key in relation.Keys)
            {
                Assert.IsTrue(closure[key].ToHashSet().SetEquals(expected[key].ToHashSet()), $"Relation differs on key {key}");
            }
        }

        [TestMethod]
        public void Diamond()
        {
            var relation = new Dictionary<int, IReadOnlyCollection<int>>()
            {
                [0] = new HashSet<int>() { 1, 2 },
                [1] = new HashSet<int>() { 3 },
                [2] = new HashSet<int>() { 3 },
                [3] = new HashSet<int>() { },
            };
            var expected = new Dictionary<int, IReadOnlyCollection<int>>()
            {
                [0] = new HashSet<int>() { 1, 2, 3 },
                [1] = new HashSet<int>() { 3 },
                [2] = new HashSet<int>() { 3 },
                [3] = new HashSet<int>() { },
            };

            var closure = relation.TransitiveClosure();

            Assert.IsTrue(closure.Keys.ToHashSet().SetEquals(expected.Keys.ToHashSet()));
            foreach (var key in relation.Keys)
            {
                Assert.IsTrue(closure[key].ToHashSet().SetEquals(expected[key].ToHashSet()), $"Relation differs on key {key}");
            }
        }
    }
}