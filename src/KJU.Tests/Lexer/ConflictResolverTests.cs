namespace KJU.Tests.Lexer
{
    using System.Collections.Generic;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConflictResolverTests
    {
        private enum TestTokenCategory
        {
            None, CategoryA, CategoryB, CategoryC
        }

        [TestMethod]
        public void TestResolveWithMaxValue()
        {
            var resolver = new ConflictResolver<TestTokenCategory>(TestTokenCategory.None);
            IEnumerable<TestTokenCategory> conflict;

            conflict = new List<TestTokenCategory> { TestTokenCategory.CategoryA, TestTokenCategory.CategoryC };
            Assert.AreEqual(TestTokenCategory.CategoryC, resolver.ResolveWithMaxValue(conflict));

            conflict = new List<TestTokenCategory> { TestTokenCategory.CategoryB };
            Assert.AreEqual(TestTokenCategory.CategoryB, resolver.ResolveWithMaxValue(conflict));

            conflict = new List<TestTokenCategory> { };
            Assert.AreEqual(TestTokenCategory.None, resolver.ResolveWithMaxValue(conflict));
        }
    }
}
