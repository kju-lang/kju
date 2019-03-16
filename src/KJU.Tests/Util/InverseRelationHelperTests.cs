namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InverseRelationHelperTests
    {
        [TestMethod]
        public void EmptyRelationTest()
        {
            var relation = new Dictionary<int, List<int>>();
            relation[0] = new List<int>();
            relation[5] = new List<int>();

            var convertedRelation = relation.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IReadOnlyCollection<int>);

            var inverseRelation = convertedRelation.InverseRelation();
            Assert.AreEqual(0, inverseRelation.Count, "Inverse relation must be of size 0!");
        }

        [TestMethod]
        public void NontrivialRelationTest()
        {
            var relation = new Dictionary<int, List<int>>();
            relation[0] = new List<int> { 0, 1, 1, 4, 5 };
            relation[4] = new List<int> { 1, 4 };
            relation[5] = new List<int> { 6, 1, 2 };

            var convertedRelation = relation.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as IReadOnlyCollection<int>);

            var inverseRelation = convertedRelation.InverseRelation();
            Assert.AreEqual(6, inverseRelation.Count, "Inverse relation must be of size 0!");

            var element = new int[] { 0, 1, 2, 4, 5, 6 };
            var expectedSecondElements = new string[]
            {
                "0",
                "0,4,5",
                "5",
                "0,4",
                "0",
                "5"
            };

            for (int i = 0; i < 6; ++i)
            {
                var secondElements = string.Join(',', inverseRelation[element[i]].OrderBy(x => x));
                Assert.AreEqual(expectedSecondElements[i], secondElements, "Second elements are not equals!");
            }
        }
    }
}