namespace KJU.Tests.Lexer
{
    using System.Collections.Generic;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConflictResolverTests
    {
        private readonly ConflictResolver<TestTokenCategory> resolver =
            new ConflictResolver<TestTokenCategory>(TestTokenCategory.None);

        private enum TestTokenCategory
        {
            None,
            CategoryA,
            CategoryB,
            CategoryC
        }

        [TestMethod]
        public void TestTwoConflicted()
        {
            var conflict = new List<TestTokenCategory> { TestTokenCategory.CategoryA, TestTokenCategory.CategoryC };
            var expected = TestTokenCategory.CategoryC;
            var actual = this.resolver.ResolveWithMaxValue(conflict);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOneConflicted()
        {
            var conflict = new List<TestTokenCategory> { TestTokenCategory.CategoryB };
            var expected = TestTokenCategory.CategoryB;
            var actual = this.resolver.ResolveWithMaxValue(conflict);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestResolveWithMaxValue()
        {
            var conflict = new List<TestTokenCategory>();
            var expected = TestTokenCategory.None;
            var actual = this.resolver.ResolveWithMaxValue(conflict);
            Assert.AreEqual(expected, actual);
        }
    }
}