namespace KJU.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SimpleLexerIntegrationTests
    {
        private enum SimpleTokenCategory
        {
            None,
            A,
            B,
            Eof
        }

        /// <summary>
        /// Test Lexer on a simple expression read from string.
        /// </summary>
        [TestMethod]
        public void TestABString()
        {
            string inputString = "abaabb";

            var tokenCategories = new List<KeyValuePair<SimpleTokenCategory, string>>
            {
                new KeyValuePair<SimpleTokenCategory, string>(SimpleTokenCategory.A, "a"),
                new KeyValuePair<SimpleTokenCategory, string>(SimpleTokenCategory.B, "b"),
            };

            IInputReader inputReader = new StringInputReader(inputString);
            var input = inputReader.Read();
            var conflictResolver = new ConflictResolver<SimpleTokenCategory>(SimpleTokenCategory.None);
            var lexer = new Lexer<SimpleTokenCategory>(
                tokenCategories,
                SimpleTokenCategory.Eof,
                SimpleTokenCategory.None,
                conflictResolver.ResolveWithMaxValue);
            var actual = lexer.Scan(input).Select(x => x.Category).ToList();
            var expected = new List<SimpleTokenCategory>
            {
                SimpleTokenCategory.A, SimpleTokenCategory.B, SimpleTokenCategory.A, SimpleTokenCategory.A,
                SimpleTokenCategory.B, SimpleTokenCategory.B, SimpleTokenCategory.Eof
            };
            var expectedText = string.Join(", ", expected);
            var actualText = string.Join(", ", actual);
            CollectionAssert.AreEqual(expected, actual, $"Expected: {expectedText}, actual: {actualText}");
        }
    }
}