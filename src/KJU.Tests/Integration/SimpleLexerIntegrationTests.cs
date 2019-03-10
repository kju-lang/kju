namespace KJU.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Tests.Integration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TC = Integration.SimpleTokenCategory;

    [TestClass]
    public class SimpleLexerIntegrationTests
    {
        /// <summary>
        /// Test Lexer on a simple expression read from string.
        /// </summary>
        [TestMethod]
        public void TestABString()
        {
            string inputString = "abaabb";

            List<KeyValuePair<TC, string>> tokenCategories = new List<KeyValuePair<TC, string>>
            {
                new KeyValuePair<TC, string>(SimpleTokenCategory.A, "a"),
                new KeyValuePair<TC, string>(SimpleTokenCategory.B, "b"),
            };

            IInputReader inputReader = new KJU.Core.Input.StringInputReader(inputString);
            List<KeyValuePair<ILocation, char>> input = inputReader.Read();
            var conf = new ConflictResolver<TC>(TC.None);
            Lexer<TC> lexer = new Lexer<TC>(tokenCategories, TC.None, conf.ResolveWithMaxValue);
            IEnumerable<Token<TC>> outputTokens = lexer.Scan(input);

            StringBuilder result = new StringBuilder();

            foreach (var t in outputTokens)
            {
                var letter = t.Category;
                switch (letter)
                {
                    case TC.A:
                        result.Append('a');
                        break;
                    case TC.B:
                        result.Append('b');
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
            }

            Assert.AreEqual(inputString, result.ToString());
        }
    }
}
