namespace KJU.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LexerIntegrationTests
    {
        private enum StringTestCategory
        {
            None = 0, Paren, Operator, Number, Variable, Whitespace, Eof
        }

        /// <summary>
        /// Test Lexer on a simple expression read from string.
        /// </summary>
        [TestMethod]
        public void TestWithStringInput()
        {
            string inputString = "(a +40)*num   +\n[(g] * -45x";

            var tokenCategories = new List<KeyValuePair<StringTestCategory, string>>
            {
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Paren,      "[\\(\\)\\[\\]]"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Operator,   "[\\*+\\-]"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Number,     "0|[1-9][0-9]*"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Variable,   "[a-z][a-z]*"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Whitespace, "[ \n][ \n]*")
            };

            var expectedTokens = new List<Token<StringTestCategory>>
            {
                CreateToken(StringTestCategory.Paren,      "(",   new StringLocation(0),  new StringLocation(1)),
                CreateToken(StringTestCategory.Variable,   "a",   new StringLocation(1),  new StringLocation(2)),
                CreateToken(StringTestCategory.Whitespace, " ",   new StringLocation(2),  new StringLocation(3)),
                CreateToken(StringTestCategory.Operator,   "+",   new StringLocation(3),  new StringLocation(4)),
                CreateToken(StringTestCategory.Number,     "40",  new StringLocation(4),  new StringLocation(6)),
                CreateToken(StringTestCategory.Paren,      ")",   new StringLocation(6),  new StringLocation(7)),
                CreateToken(StringTestCategory.Operator,   "*",   new StringLocation(7),  new StringLocation(8)),
                CreateToken(StringTestCategory.Variable,   "num", new StringLocation(8),  new StringLocation(11)),
                CreateToken(StringTestCategory.Whitespace, "   ", new StringLocation(11), new StringLocation(14)),
                CreateToken(StringTestCategory.Operator,   "+",   new StringLocation(14), new StringLocation(15)),
                CreateToken(StringTestCategory.Whitespace, "\n",  new StringLocation(15), new StringLocation(16)),
                CreateToken(StringTestCategory.Paren,      "[",   new StringLocation(16), new StringLocation(17)),
                CreateToken(StringTestCategory.Paren,      "(",   new StringLocation(17), new StringLocation(18)),
                CreateToken(StringTestCategory.Variable,   "g",   new StringLocation(18), new StringLocation(19)),
                CreateToken(StringTestCategory.Paren,      "]",   new StringLocation(19), new StringLocation(20)),
                CreateToken(StringTestCategory.Whitespace, " ",   new StringLocation(20), new StringLocation(21)),
                CreateToken(StringTestCategory.Operator,   "*",   new StringLocation(21), new StringLocation(22)),
                CreateToken(StringTestCategory.Whitespace, " ",   new StringLocation(22), new StringLocation(23)),
                CreateToken(StringTestCategory.Operator,   "-",   new StringLocation(23), new StringLocation(24)),
                CreateToken(StringTestCategory.Number,     "45",  new StringLocation(24), new StringLocation(26)),
                CreateToken(StringTestCategory.Variable,   "x",   new StringLocation(26), new StringLocation(27)),
                CreateToken(StringTestCategory.Eof,        null,  null,                            null)
            };

            IInputReader inputReader = new StringInputReader(inputString);
            var resolver = new ConflictResolver<StringTestCategory>(StringTestCategory.None);
            var lexer = new Lexer<StringTestCategory>(tokenCategories, StringTestCategory.Eof, StringTestCategory.None, resolver.ResolveWithMaxValue);
            var outputTokens = lexer.Scan(inputReader.Read());
            Assert.IsTrue(outputTokens.SequenceEqual(expectedTokens, new TokenComparer<StringTestCategory>()));
        }

        [SuppressMessage(
            "StyleCop.CSharp.OrderingRules",
            "SA1201:ElementsMustAppearInTheCorrectOrder",
            Justification="This enum type should be grouped with the following test method")]
        private enum FileTestCategory
        {
            None = 0, Brace, Bracket, Colon, Comma, Minus, Number, Boolean, Null, QuotedString, Whitespace, Eof
        }

        /// <summary>
        /// Test Lexer on JSON-like file -
        /// (limits allowed key/string characters, no escapes, simpler numbers).
        /// </summary>
        [TestMethod]
        public void TestWithFileInput()
        {
            string filename = GetFullPathToFile(Path.Combine("Integration", "pseudoJSONSample.txt"));

            /*
            Input from file:

            "[ {\"2Ws8P0wYj\":  -17, \"dbV\":   true,\"B5b0BofwT\":  -13.607189896949151 }, [], \tnull,    [], \n{\n\"NwAssf8pU\":null  }\n  ]"
            */

            var tokenCategories = new List<KeyValuePair<FileTestCategory, string>>
            {
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Brace,        "{|}"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Bracket,      "\\[|\\]"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Colon,        ":"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Comma,        ","),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Minus,        "[\\-]"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Number,       "[1-9][0-9]*|[1-9][0-9]*.[0-9][0-9]*|0.[0-9][0-9]*"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Boolean,      "true|false"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Null,         "null"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.QuotedString, "\"[a-zA-Z0-9_]*\""),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Whitespace,   "[ \n\r\t\v][ \n\r\t\v]*")
            };

            var expectedTokens = new List<Token<FileTestCategory>>
            {
                CreateToken(FileTestCategory.Bracket,      "[",                  new FileLocation(filename, 1, 1),  new FileLocation(filename, 1, 2)),
                CreateToken(FileTestCategory.Whitespace,   " ",                  new FileLocation(filename, 1, 2),  new FileLocation(filename, 1, 3)),
                CreateToken(FileTestCategory.Brace,        "{",                  new FileLocation(filename, 1, 3),  new FileLocation(filename, 1, 4)),
                CreateToken(FileTestCategory.QuotedString, "\"2Ws8P0wYj\"",      new FileLocation(filename, 1, 4),  new FileLocation(filename, 1, 15)),
                CreateToken(FileTestCategory.Colon,        ":",                  new FileLocation(filename, 1, 15), new FileLocation(filename, 1, 16)),
                CreateToken(FileTestCategory.Whitespace,   "  ",                 new FileLocation(filename, 1, 16), new FileLocation(filename, 1, 18)),
                CreateToken(FileTestCategory.Minus,        "-",                  new FileLocation(filename, 1, 18), new FileLocation(filename, 1, 19)),
                CreateToken(FileTestCategory.Number,       "17",                 new FileLocation(filename, 1, 19), new FileLocation(filename, 1, 21)),
                CreateToken(FileTestCategory.Comma,        ",",                  new FileLocation(filename, 1, 21), new FileLocation(filename, 1, 22)),
                CreateToken(FileTestCategory.Whitespace,   " ",                  new FileLocation(filename, 1, 22), new FileLocation(filename, 1, 23)),
                CreateToken(FileTestCategory.QuotedString, "\"dbV\"",            new FileLocation(filename, 1, 23), new FileLocation(filename, 1, 28)),
                CreateToken(FileTestCategory.Colon,        ":",                  new FileLocation(filename, 1, 28), new FileLocation(filename, 1, 29)),
                CreateToken(FileTestCategory.Whitespace,   "   ",                new FileLocation(filename, 1, 29), new FileLocation(filename, 1, 32)),
                CreateToken(FileTestCategory.Boolean,      "true",               new FileLocation(filename, 1, 32), new FileLocation(filename, 1, 36)),
                CreateToken(FileTestCategory.Comma,        ",",                  new FileLocation(filename, 1, 36), new FileLocation(filename, 1, 37)),
                CreateToken(FileTestCategory.QuotedString, "\"B5b0BofwT\"",      new FileLocation(filename, 1, 37), new FileLocation(filename, 1, 48)),
                CreateToken(FileTestCategory.Colon,        ":",                  new FileLocation(filename, 1, 48), new FileLocation(filename, 1, 49)),
                CreateToken(FileTestCategory.Whitespace,   "  ",                 new FileLocation(filename, 1, 49), new FileLocation(filename, 1, 51)),
                CreateToken(FileTestCategory.Minus,        "-",                  new FileLocation(filename, 1, 51), new FileLocation(filename, 1, 52)),
                CreateToken(FileTestCategory.Number,       "13.607189896949151", new FileLocation(filename, 1, 52), new FileLocation(filename, 1, 70)),
                CreateToken(FileTestCategory.Whitespace,   " ",                  new FileLocation(filename, 1, 70), new FileLocation(filename, 1, 71)),
                CreateToken(FileTestCategory.Brace,        "}",                  new FileLocation(filename, 1, 71), new FileLocation(filename, 1, 72)),
                CreateToken(FileTestCategory.Comma,        ",",                  new FileLocation(filename, 1, 72), new FileLocation(filename, 1, 73)),
                CreateToken(FileTestCategory.Whitespace,   " ",                  new FileLocation(filename, 1, 73), new FileLocation(filename, 1, 74)),
                CreateToken(FileTestCategory.Bracket,      "[",                  new FileLocation(filename, 1, 74), new FileLocation(filename, 1, 75)),
                CreateToken(FileTestCategory.Bracket,      "]",                  new FileLocation(filename, 1, 75), new FileLocation(filename, 1, 76)),
                CreateToken(FileTestCategory.Comma,        ",",                  new FileLocation(filename, 1, 76), new FileLocation(filename, 1, 77)),
                CreateToken(FileTestCategory.Whitespace,   " \t",                new FileLocation(filename, 1, 77), new FileLocation(filename, 1, 79)),
                CreateToken(FileTestCategory.Null,         "null",               new FileLocation(filename, 1, 79), new FileLocation(filename, 1, 83)),
                CreateToken(FileTestCategory.Comma,        ",",                  new FileLocation(filename, 1, 83), new FileLocation(filename, 1, 84)),
                CreateToken(FileTestCategory.Whitespace,   "    ",               new FileLocation(filename, 1, 84), new FileLocation(filename, 1, 88)),
                CreateToken(FileTestCategory.Bracket,      "[",                  new FileLocation(filename, 1, 88), new FileLocation(filename, 1, 89)),
                CreateToken(FileTestCategory.Bracket,      "]",                  new FileLocation(filename, 1, 89), new FileLocation(filename, 1, 90)),
                CreateToken(FileTestCategory.Comma,        ",",                  new FileLocation(filename, 1, 90), new FileLocation(filename, 1, 91)),
                CreateToken(FileTestCategory.Whitespace,   " \n",                new FileLocation(filename, 1, 91), new FileLocation(filename, 2, 1)),
                CreateToken(FileTestCategory.Brace,        "{",                  new FileLocation(filename, 2, 1),  new FileLocation(filename, 2, 2)),
                CreateToken(FileTestCategory.Whitespace,   "\n  ",               new FileLocation(filename, 2, 2),  new FileLocation(filename, 3, 3)),
                CreateToken(FileTestCategory.QuotedString, "\"NwAssf8pU\"",      new FileLocation(filename, 3, 3),  new FileLocation(filename, 3, 14)),
                CreateToken(FileTestCategory.Colon,        ":",                  new FileLocation(filename, 3, 14), new FileLocation(filename, 3, 15)),
                CreateToken(FileTestCategory.Null,         "null",               new FileLocation(filename, 3, 15), new FileLocation(filename, 3, 19)),
                CreateToken(FileTestCategory.Whitespace,   "  ",                 new FileLocation(filename, 3, 19), new FileLocation(filename, 3, 21)),
                CreateToken(FileTestCategory.Brace,        "}",                  new FileLocation(filename, 3, 21), new FileLocation(filename, 3, 22)),
                CreateToken(FileTestCategory.Whitespace,   "\n  ",               new FileLocation(filename, 3, 22), new FileLocation(filename, 4, 3)),
                CreateToken(FileTestCategory.Bracket,      "]",                  new FileLocation(filename, 4, 3),  new FileLocation(filename, 4, 4)),
                CreateToken(FileTestCategory.Whitespace,   "\n",                 new FileLocation(filename, 4, 4),  new FileLocation(filename, 5, 1)),
                CreateToken(FileTestCategory.Eof,   null,                 null,  null)
            };

            IInputReader inputReader = new FileInputReader(filename);
            var resolver = new ConflictResolver<FileTestCategory>(FileTestCategory.None);
            var lexer = new Lexer<FileTestCategory>(tokenCategories, FileTestCategory.Eof, FileTestCategory.None, resolver.ResolveWithMaxValue);
            var outputTokens = lexer.Scan(inputReader.Read());
            var expectedText = string.Join(",\n", expectedTokens);
            var actualText = string.Join(",\n", outputTokens);
            Assert.IsTrue(outputTokens.SequenceEqual(expectedTokens, new TokenComparer<FileTestCategory>()), $"Expected:\n{expectedText}\nactual:\n{actualText}");
        }

        [SuppressMessage(
            "StyleCop.CSharp.OrderingRules",
            "SA1201:ElementsMustAppearInTheCorrectOrder",
            Justification="This enum type should be grouped with the following test method")]
        private enum CommentsTestCategory
        {
            None = 0, UppercaseWord, LowercaseWord, Punctuation, Space, Eof
        }

        /// <summary>
        /// Test Lexer on input passed through comment removal.
        /// </summary>
        [TestMethod]
        public void TestWithRemovedComments()
        {
            string inputString = "Velit qui eu cillum anim /*labore. Mollit nulla " +
                                 "consequat fugiat ut - dolor nost/*rud veniam fugiat adipisicing " +
                                 "irure est cupi*/datat, minim. Incid*/idunt occaecat ipsum officia. " +
                                 "Consectetur labore volup/*tate voluptate inci*/didunt, eu occaecat.";

            var tokenCategories = new List<KeyValuePair<CommentsTestCategory, string>>
            {
                new KeyValuePair<CommentsTestCategory, string>(CommentsTestCategory.UppercaseWord, "[A-Z][a-z]*"),
                new KeyValuePair<CommentsTestCategory, string>(CommentsTestCategory.LowercaseWord, "[a-z][a-z]*"),
                new KeyValuePair<CommentsTestCategory, string>(CommentsTestCategory.Punctuation,   ".|[,\\-]"),
                new KeyValuePair<CommentsTestCategory, string>(CommentsTestCategory.Space,         "  *")
            };

            var expectedTokens = new List<Token<CommentsTestCategory>>
            {
                CreateToken(CommentsTestCategory.UppercaseWord, "Velit",       new StringLocation(0),   new StringLocation(5)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(5),   new StringLocation(6)),
                CreateToken(CommentsTestCategory.LowercaseWord, "qui",         new StringLocation(6),   new StringLocation(9)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(9),   new StringLocation(10)),
                CreateToken(CommentsTestCategory.LowercaseWord, "eu",          new StringLocation(10),  new StringLocation(12)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(12),  new StringLocation(13)),
                CreateToken(CommentsTestCategory.LowercaseWord, "cillum",      new StringLocation(13),  new StringLocation(19)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(19),  new StringLocation(20)),
                CreateToken(CommentsTestCategory.LowercaseWord, "anim",        new StringLocation(20),  new StringLocation(24)),
                CreateToken(CommentsTestCategory.Space,         "  ",          new StringLocation(24),  new StringLocation(149)),
                CreateToken(CommentsTestCategory.LowercaseWord, "idunt",       new StringLocation(149), new StringLocation(154)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(154), new StringLocation(155)),
                CreateToken(CommentsTestCategory.LowercaseWord, "occaecat",    new StringLocation(155), new StringLocation(163)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(163), new StringLocation(164)),
                CreateToken(CommentsTestCategory.LowercaseWord, "ipsum",       new StringLocation(164), new StringLocation(169)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(169), new StringLocation(170)),
                CreateToken(CommentsTestCategory.LowercaseWord, "officia",     new StringLocation(170), new StringLocation(177)),
                CreateToken(CommentsTestCategory.Punctuation,   ".",           new StringLocation(177), new StringLocation(178)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(178), new StringLocation(179)),
                CreateToken(CommentsTestCategory.UppercaseWord, "Consectetur", new StringLocation(179), new StringLocation(190)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(190), new StringLocation(191)),
                CreateToken(CommentsTestCategory.LowercaseWord, "labore",      new StringLocation(191), new StringLocation(197)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(197), new StringLocation(198)),
                CreateToken(CommentsTestCategory.LowercaseWord, "volup",       new StringLocation(198), new StringLocation(203)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(203), new StringLocation(226)),
                CreateToken(CommentsTestCategory.LowercaseWord, "didunt",      new StringLocation(226), new StringLocation(232)),
                CreateToken(CommentsTestCategory.Punctuation,   ",",           new StringLocation(232), new StringLocation(233)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(233), new StringLocation(234)),
                CreateToken(CommentsTestCategory.LowercaseWord, "eu",          new StringLocation(234), new StringLocation(236)),
                CreateToken(CommentsTestCategory.Space,         " ",           new StringLocation(236), new StringLocation(237)),
                CreateToken(CommentsTestCategory.LowercaseWord, "occaecat",    new StringLocation(237), new StringLocation(245)),
                CreateToken(CommentsTestCategory.Punctuation,   ".",           new StringLocation(245), new StringLocation(246)),
                CreateToken(CommentsTestCategory.Eof,           null,          null,                             null),
            };

            StringInputReader inputReader = new StringInputReader(inputString);
            Preprocessor preprocessor = new Preprocessor();
            var input = preprocessor.PreprocessInput(inputReader.ReadGenerator());
            var resolver = new ConflictResolver<CommentsTestCategory>(CommentsTestCategory.None);
            var lexer = new Lexer<CommentsTestCategory>(tokenCategories, CommentsTestCategory.Eof, CommentsTestCategory.None, resolver.ResolveWithMaxValue);
            var outputTokens = lexer.Scan(input);
            var expectedText = string.Join(",\n", expectedTokens);
            var actualText = string.Join(",\n", outputTokens);
            Assert.IsTrue(outputTokens.SequenceEqual(expectedTokens, new TokenComparer<CommentsTestCategory>()), $"Expected:\n{expectedText}\nactual:\n{actualText}");
        }

        // pathRelative is relative to src/KJU.Tests
        // see https://stackoverflow.com/questions/23826773/how-do-i-make-a-data-file-available-to-unit-tests/53004985#53004985
        // changed to use cross-platform delimiters
        private static string GetFullPathToFile(string pathRelative)
        {
            string pathAssembly = Assembly.GetExecutingAssembly().Location;
            string folderAssembly = Path.GetDirectoryName(pathAssembly);
            string combinedPath = Path.Combine(folderAssembly, "..", "..", "..", pathRelative);
            return Path.GetFullPath(combinedPath);
        }

        private static Token<TLabel> CreateToken<TLabel>(TLabel category, string text, ILocation s, ILocation t)
        {
            Range inputRange = null;
            if (s != null || t != null)
            {
                inputRange = new Range { Begin = s, End = t };
            }

            var token = new Token<TLabel>
            {
                Category = category, Text = text, InputRange = inputRange
            };
            return token;
        }

        private class TokenComparer<TLabel> : IEqualityComparer<Token<TLabel>>
            where TLabel : IComparable
        {
            public bool Equals(Token<TLabel> x, Token<TLabel> y)
            {
                return x.Category.CompareTo(y.Category) == 0
                       && x.Text == y.Text && (x.InputRange == null ||
                                               (x.InputRange.Begin.ToString() == y.InputRange.Begin.ToString()
                                                && x.InputRange.End.ToString() == y.InputRange.End.ToString()));
            }

            public int GetHashCode(Token<TLabel> obj)
            {
                if (obj.InputRange == null)
                {
                    return Tuple.Create(
                        obj.Category,
                        obj.Text).GetHashCode();
                }

                return Tuple.Create(
                    obj.Category,
                    obj.Text,
                    obj.InputRange.Begin.ToString(),
                    obj.InputRange.End.ToString()).GetHashCode();
            }
        }
    }
}
