namespace KJU.Tests.Util
{
    using System.Collections.Generic;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GraphReverserTest
    {
        [TestMethod]
        public void SimpleTest()
        {
            var graph = new Dictionary<int, IReadOnlyCollection<int>>()
            {
                [0] = new HashSet<int>() { 1, 2 },
                [1] = new HashSet<int>() { },
                [2] = new HashSet<int>() { 1 },
            };

            var expectedReverse = new Dictionary<int, IReadOnlyCollection<int>>()
            {
                [0] = new HashSet<int>() { },
                [1] = new HashSet<int>() { 0, 2 },
                [2] = new HashSet<int>() { 0 },
            };

            var result = GraphReverser.ReverseGraph(graph);

            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(expectedReverse, result));
        }
    }
}