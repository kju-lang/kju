namespace KJU.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LexerIntegrationTests
    {
        private enum StringTestCategory
        {
            None = 0, Paren, Operator, Number, Variable, Whitespace
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
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Paren,      "[()\\[\\]]"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Operator,   "[*+-]"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Number,     "0|[1-9][0-9]*"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Variable,   "[a-z][a-z]*"),
                new KeyValuePair<StringTestCategory, string>(StringTestCategory.Whitespace, "[ \\n][ \\n]*"),
            };

            var expectedTokens = new List<Token<StringTestCategory>>
            {
                CreateToken<StringTestCategory>(StringTestCategory.Paren,       "("),
                CreateToken<StringTestCategory>(StringTestCategory.Variable,    "a"),
                CreateToken<StringTestCategory>(StringTestCategory.Whitespace,  " "),
                CreateToken<StringTestCategory>(StringTestCategory.Operator,    "+"),
                CreateToken<StringTestCategory>(StringTestCategory.Number,      "40"),
                CreateToken<StringTestCategory>(StringTestCategory.Paren,       ")"),
                CreateToken<StringTestCategory>(StringTestCategory.Operator,    "*"),
                CreateToken<StringTestCategory>(StringTestCategory.Variable,    "num"),
                CreateToken<StringTestCategory>(StringTestCategory.Whitespace,  "   "),
                CreateToken<StringTestCategory>(StringTestCategory.Operator,    "+"),
                CreateToken<StringTestCategory>(StringTestCategory.Whitespace,  "\n"),
                CreateToken<StringTestCategory>(StringTestCategory.Paren,       "["),
                CreateToken<StringTestCategory>(StringTestCategory.Paren,       "("),
                CreateToken<StringTestCategory>(StringTestCategory.Variable,    "g"),
                CreateToken<StringTestCategory>(StringTestCategory.Paren,       "]"),
                CreateToken<StringTestCategory>(StringTestCategory.Whitespace,  " "),
                CreateToken<StringTestCategory>(StringTestCategory.Operator,    "*"),
                CreateToken<StringTestCategory>(StringTestCategory.Whitespace,  " "),
                CreateToken<StringTestCategory>(StringTestCategory.Operator,    "-"),
                CreateToken<StringTestCategory>(StringTestCategory.Number,      "45"),
                CreateToken<StringTestCategory>(StringTestCategory.Variable,    "x"),
            };

            IInputReader inputReader = new StringInputReader(inputString);
            var resolver = new ConflictResolver<StringTestCategory>(StringTestCategory.None);
            var lexer = new Lexer<StringTestCategory>(tokenCategories, resolver.ResolveWithMaxValue);
            var outputTokens = lexer.Scan(inputReader.Read());
            Assert.IsTrue(outputTokens.SequenceEqual(expectedTokens, new TokenComparer<StringTestCategory>()));
        }

        [SuppressMessage(
            "StyleCop.CSharp.OrderingRules",
            "SA1201:ElementsMustAppearInTheCorrectOrder",
            Justification="This enum type should be grouped with the following test method")]
        private enum FileTestCategory
        {
            None = 0, Brace, Bracket, Colon, Comma, Minus, Number, Boolean, Null, QuotedString, Whitespace
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
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Minus,        "-"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Number,       "[1-9][0-9]*|[1-9][0-9]*.[0-9][0-9]*|0.[0-9][0-9]*"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Boolean,      "true|false"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Null,         "null"),
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.QuotedString, "\"[a-zA-Z0-9_]*\""),

                // TODO: check if string-to-regex expects actual whitespace characters or escape codes
                //       unescape backslashes if it's the latter case
                new KeyValuePair<FileTestCategory, string>(FileTestCategory.Whitespace,   "[ \\n\\r\\t\\v][ \\n\\r\\t\\v]*"),
            };

            var expectedTokens = new List<Token<FileTestCategory>>
            {
                CreateToken<FileTestCategory>(FileTestCategory.Bracket,      "["),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   " "),
                CreateToken<FileTestCategory>(FileTestCategory.Brace,        "{"),
                CreateToken<FileTestCategory>(FileTestCategory.QuotedString, "\"2Ws8P0wYj\""),
                CreateToken<FileTestCategory>(FileTestCategory.Colon,        ":"),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   "  "),
                CreateToken<FileTestCategory>(FileTestCategory.Minus,        "-"),
                CreateToken<FileTestCategory>(FileTestCategory.Number,       "17"),
                CreateToken<FileTestCategory>(FileTestCategory.Comma,        ","),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   " "),
                CreateToken<FileTestCategory>(FileTestCategory.QuotedString, "\"dbV\""),
                CreateToken<FileTestCategory>(FileTestCategory.Colon,        ":"),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   "   "),
                CreateToken<FileTestCategory>(FileTestCategory.Boolean,      "true"),
                CreateToken<FileTestCategory>(FileTestCategory.Comma,        ","),
                CreateToken<FileTestCategory>(FileTestCategory.QuotedString, "\"B5b0BofwT\""),
                CreateToken<FileTestCategory>(FileTestCategory.Colon,        ":"),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   "  "),
                CreateToken<FileTestCategory>(FileTestCategory.Minus,        "-"),
                CreateToken<FileTestCategory>(FileTestCategory.Number,       "13.607189896949151"),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   " "),
                CreateToken<FileTestCategory>(FileTestCategory.Brace,        "}"),
                CreateToken<FileTestCategory>(FileTestCategory.Comma,        ","),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   " "),
                CreateToken<FileTestCategory>(FileTestCategory.Bracket,      "["),
                CreateToken<FileTestCategory>(FileTestCategory.Bracket,      "]"),
                CreateToken<FileTestCategory>(FileTestCategory.Comma,        ","),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   " \t"),
                CreateToken<FileTestCategory>(FileTestCategory.Null,         "null"),
                CreateToken<FileTestCategory>(FileTestCategory.Comma,        ","),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   "    "),
                CreateToken<FileTestCategory>(FileTestCategory.Bracket,      "["),
                CreateToken<FileTestCategory>(FileTestCategory.Bracket,      "]"),
                CreateToken<FileTestCategory>(FileTestCategory.Comma,        ","),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   " \n"),
                CreateToken<FileTestCategory>(FileTestCategory.Brace,        "{"),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   "\n"),
                CreateToken<FileTestCategory>(FileTestCategory.QuotedString, "\"NwAssf8pU\""),
                CreateToken<FileTestCategory>(FileTestCategory.Colon,        ":"),
                CreateToken<FileTestCategory>(FileTestCategory.Null,         "null"),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   "  "),
                CreateToken<FileTestCategory>(FileTestCategory.Brace,        "}"),
                CreateToken<FileTestCategory>(FileTestCategory.Whitespace,   "\n  "),
                CreateToken<FileTestCategory>(FileTestCategory.Bracket,      "]"),
            };

            IInputReader inputReader = new FileInputReader(filename);
            var resolver = new ConflictResolver<FileTestCategory>(FileTestCategory.None);
            var lexer = new Lexer<FileTestCategory>(tokenCategories, resolver.ResolveWithMaxValue);
            var outputTokens = lexer.Scan(inputReader.Read());
            Assert.IsTrue(outputTokens.SequenceEqual(expectedTokens, new TokenComparer<FileTestCategory>()));
        }

        [SuppressMessage(
            "StyleCop.CSharp.OrderingRules",
            "SA1201:ElementsMustAppearInTheCorrectOrder",
            Justification="This enum type should be grouped with the following test method")]
        private enum CommentsTestCategory
        {
            None = 0, UppercaseWord, LowercaseWord, Punctuation, Space
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
                new KeyValuePair<CommentsTestCategory, string>(CommentsTestCategory.Punctuation,   ".|[,-]"),
                new KeyValuePair<CommentsTestCategory, string>(CommentsTestCategory.Space,         " "),
            };

            var expectedTokens = new List<Token<CommentsTestCategory>>
            {
                CreateToken<CommentsTestCategory>(CommentsTestCategory.UppercaseWord, "Velit"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "qui"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "eu"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "cillum"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "anim"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "idunt"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "occaecat"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "ipsum"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "officia"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Punctuation,   "."),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.UppercaseWord, "Consectetur"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "labore"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "volup"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "didunt"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Punctuation,   ","),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "eu"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Space,         " "),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.LowercaseWord, "occaecat"),
                CreateToken<CommentsTestCategory>(CommentsTestCategory.Punctuation,   "."),
            };

            StringInputReader inputReader = new StringInputReader(inputString);
            Preprocessor preprocessor = new Preprocessor();
            var input = preprocessor.PreprocessInput(inputReader.ReadGenerator());
            var resolver = new ConflictResolver<CommentsTestCategory>(CommentsTestCategory.None);
            var lexer = new Lexer<CommentsTestCategory>(tokenCategories, resolver.ResolveWithMaxValue);
            var outputTokens = lexer.Scan(input);
            Assert.IsTrue(outputTokens.SequenceEqual(expectedTokens, new TokenComparer<CommentsTestCategory>()));
        }

        // pathRelative is relative to src/KJU.Tests
        // see https://stackoverflow.com/questions/23826773/how-do-i-make-a-data-file-available-to-unit-tests/53004985#53004985
        // changed to use cross-platform delimiters
        private static string GetFullPathToFile(string pathRelative)
        {
            string pathAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string folderAssembly = Path.GetDirectoryName(pathAssembly);
            string combinedPath = Path.Combine(folderAssembly, "..", "..", "..", pathRelative);
            return Path.GetFullPath(combinedPath);
        }

        private static Token<TLabel> CreateToken<TLabel>(TLabel category, string text)
        {
            var token = new Token<TLabel>();
            token.Category = category;
            token.Text = text;
            return token;
        }

        // TODO: Take token.InputRange into account
        private class TokenComparer<TLabel> : IEqualityComparer<Token<TLabel>>
            where TLabel : System.IComparable
        {
            public bool Equals(Token<TLabel> x, Token<TLabel> y)
            {
                return x.Category.CompareTo(y.Category) == 0
                    && x.Text == y.Text;
            }

            public int GetHashCode(Token<TLabel> obj)
            {
                return System.Tuple.Create(obj.Category, obj.Text).GetHashCode();
            }
        }
    }
}
