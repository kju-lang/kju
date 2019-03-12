namespace KJU.Tests.Regex.StringToRegexConverter
{
    using KJU.Core.Regex;
    using KJU.Core.Regex.StringToRegexConverter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // Using of pragma improves tests readability.
    [TestClass]
#pragma warning disable SA1118
    public class StringToRegexConverterTests
    {
        private readonly IStringToRegexConverter converter;

        public StringToRegexConverterTests()
        {
            var factory = new StringToRegexConverterFactory();
            this.converter = factory.CreateConverter();
        }

        [TestMethod]
        public void SimpleAtom()
        {
            const string input = "a";
            var expected = new AtomicRegex<char>('a');
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AtomAndStar()
        {
            const string input = "a*";
            var expected = new StarRegex<char>(new AtomicRegex<char>('a'));
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Sum()
        {
            const string input = "a|b|c|d";
            var expected = new SumRegex<char>(
                new SumRegex<char>(
                    new SumRegex<char>(
                        new AtomicRegex<char>('a'),
                        new AtomicRegex<char>('b')),
                    new AtomicRegex<char>('c')),
                new AtomicRegex<char>('d'));
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CharacterClass()
        {
            const string input = "[a-d]";
            var expected = new SumRegex<char>(
                new SumRegex<char>(
                    new SumRegex<char>(
                        new AtomicRegex<char>('a'),
                        new AtomicRegex<char>('b')),
                    new AtomicRegex<char>('c')),
                new AtomicRegex<char>('d'));
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NotClosedCharacterClass()
        {
            const string input = "[a-d";
            Assert.ThrowsException<RegexParseException>(() => this.converter.Convert(input));
        }

        [TestMethod]
        public void MinusAtEndOfCharacterClass()
        {
            const string input = "[a-]";
            Assert.ThrowsException<RegexParseException>(() => this.converter.Convert(input));
        }

        [TestMethod]
        public void MinusAtBeginningOfCharacterClass()
        {
            const string input = "[-d]";
            Assert.ThrowsException<RegexParseException>(() => this.converter.Convert(input));
        }

        [TestMethod]
        public void Catenation()
        {
            const string input = "abcd";
            var expected = new ConcatRegex<char>(
                new ConcatRegex<char>(
                    new ConcatRegex<char>(
                        new AtomicRegex<char>('a'),
                        new AtomicRegex<char>('b')),
                    new AtomicRegex<char>('c')), new AtomicRegex<char>('d'));
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void StarEscape()
        {
            const string input = "\\*";
            var expected = new AtomicRegex<char>('*');
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EscapeInCharacterClass()
        {
            const string input = "[\\-]";
            var expected = new AtomicRegex<char>('-');
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Brackets()
        {
            const string input = "(a)";
            var expected = new AtomicRegex<char>('a');
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BackslashAtTheEndOfInput()
        {
            const string input = "\\";
            Assert.ThrowsException<RegexParseException>(() => this.converter.Convert(input));
        }

        [TestMethod]
        public void EpsilonRegex()
        {
            const string input = "";
            var expected = new EpsilonRegex<char>();
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SumAndEpsilonRegex()
        {
            const string input = "a|";
            var expected = new SumRegex<char>(new AtomicRegex<char>('a'), new EpsilonRegex<char>());
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BracketsEpsilonRegex()
        {
            const string input = "()";
            var expected = new EpsilonRegex<char>();
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EpsilonRegexInsideSum()
        {
            const string input = "a||b";
            var expected = new SumRegex<char>(
                new SumRegex<char>(
                    new AtomicRegex<char>('a'),
                    new EpsilonRegex<char>()),
                new AtomicRegex<char>('b'));
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EmptyCharacterClass()
        {
            const string input = "[]";
            var expected = new EmptyRegex<char>();
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NotMatchingBrackets()
        {
            const string input = "((())";
            Assert.ThrowsException<RegexParseException>(() => this.converter.Convert(input));
        }

        // ASCII:
        // 91 - [
        // 92 - \
        // 93 - ]
        [TestMethod]
        public void CharacterClassEscapedBrackets()
        {
            const string input = "[\\[-\\]]";
            var expected = new SumRegex<char>(new SumRegex<char>(new AtomicRegex<char>('['), new AtomicRegex<char>('\\')), new AtomicRegex<char>(']'));
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EmptyCharacterRange()
        {
            const string input = "[a-Z]";
            Assert.ThrowsException<RegexParseException>(() => this.converter.Convert(input));
        }

        [TestMethod]
        public void MinusCharacterRegex()
        {
            const string input = "\\-";
            var expected = new AtomicRegex<char>('-');
            var actual = this.converter.Convert(input);
            Assert.AreEqual(expected, actual);
        }
    }
}