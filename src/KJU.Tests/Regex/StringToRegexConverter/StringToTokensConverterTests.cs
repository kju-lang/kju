namespace KJU.Tests.Regex
{
    using System.Collections.Generic;
    using KJU.Core.Regex.StringToRegexConverter;
    using KJU.Core.Regex.StringToRegexConverter.Tokens;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringToTokensConverterTests
    {
        private IStringToTokensConverter converter = new StringToTokensConverter();

        [TestMethod]
        public void SimpleAtom()
        {
            const string input = "a";
            var expected = new List<Token> { new CharacterClassToken("a") };
            var actual = this.converter.Convert(input);
            var expectedToString = string.Join(", ", expected);
            var actualToString = string.Join(", ", actual);
            CollectionAssert.AreEqual(expected, actual, $"Expected: {expectedToString}, actual: {actualToString}");
        }

        [TestMethod]
        public void EscapedBracketsCharacterClass()
        {
            const string input = "[\\[-\\]]";
            var expected = new List<Token> { new CharacterClassToken("\\[-\\]") };
            var actual = this.converter.Convert(input);
            var expectedToString = string.Join(", ", expected);
            var actualToString = string.Join(", ", actual);
            CollectionAssert.AreEqual(expected, actual, $"Expected: {expectedToString}, actual: {actualToString}");
        }
    }
}