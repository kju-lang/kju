namespace KJU.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LexerIntegrationTests
    {
        /// <summary>
        /// Test Lexer on a simple expression read from string.
        /// </summary>
        [TestMethod]
        public void TestStringInputOnMath()
        {
            string inputString = "(a +40)*num   +\n[(g] * -45x";

            List<KeyValuePair<TokenCategory, string>> tokenCategories = new List<KeyValuePair<TokenCategory, string>>
            {
                /*
                paren:      "[()\[\]]"
                operator:   "[*+-]"
                number:     "0|[1-9][0-9]*"
                variable:   "[a-z][a-z]*"
                whitespace: "[ \n][ \n]*"

                            (unescaped)
                */
            };

            List<Token> expectedTokens = new List<Token>
            {
                /*
                paren,      "(",    0
                variable,   "a",    1
                whitespace, " ",    2
                operator,   "+",    3
                number,     "40",   4
                paren,      ")",    6
                operator,   "*",    7
                variable,   "num",  8
                whitespace, "   ", 11
                operator,   "+",   14
                whitespace, "\n",  15
                paren,      "[",   16
                paren,      "(",   17
                variable,   "g",   18
                paren,      "]",   19
                whitespace, " ",   20
                operator,   "*",   21
                whitespace, " ",   22
                operator,   "-",   23
                number,     "45",  24
                variable,   "x",   26
                */
            };

            InputReader inputReader = new InputReader();
            List<KeyValuePair<ILocation, char>> input = inputReader.InputFromString(inputString);
            Lexer lexer = new Lexer(tokenCategories);
            IEnumerable<Token> outputTokens = lexer.Scan(input);
            Assert.IsTrue(outputTokens.SequenceEqual(expectedTokens)); // TODO: token comparison
        }

        /// <summary>
        /// Test Lexer on JSON-like file -
        /// (limits allowed key/string characters, no escapes, simpler numbers).
        /// </summary>
        [TestMethod]
        public void TestFileInputOnJSON()
        {
            string filename = string.Empty; // TODO: figure out file access in tests

            /*
            Input from file:

            "[ {\"2Ws8P0wYj\":  -17, \"dbV\":   true,\"B5b0BofwT\":  -13.607189896949151 }, [], \tnull,    [], \n{\n\"NwAssf8pU\":null  }\n  ]"
            (quotes unescaped)

            Token categories:

            brace:        "{|}"
            bracket:      "\[|\]"
            colon:        ":"
            comma:        ","
            minus:        "-"
            number:       "[1-9][0-9]*|[1-9][0-9]*.[0-9][0-9]*|0.[0-9][0-9]*"
            boolean:      "true|false"
            null:         "null"
            quotedString: ""[a-zA-Z0-9_]*""
            whitespace:   "[ \n\r\t\v][ \n\r\t\v]*"

            Tokens:

            bracket,      "[",                    0
            whitespace,   " ",                    1
            brace,        "{",                    2
            quotedString, "\"2Ws8P0wYj\"",        3
            colon,        ":",                   14
            whitespace,   "  ",                  15
            minus,        "-",                   17
            number,       "17",                  18
            comma,        ",",                   20
            whitespace,   " ",                   21
            quotedString, "\"dbV\"",             22
            colon,        ":",                   27
            whitespace,   "   ",                 28
            boolean,      "true",                31
            comma,        ",",                   35
            quotedString, "\"B5b0BofwT\"",       36
            colon,        ":",                   47
            whitespace,   "  ",                  48
            minus,        "-",                   50
            number,       "13.607189896949151",  51
            whitespace,   " ",                   69
            brace,        "}",                   70
            comma,        ",",                   71
            whitespace,   " ",                   72
            bracket,      "[",                   73
            bracket,      "]",                   74
            comma,        ",",                   75
            whitespace,   " \t",                 76
            null,         "null",                78
            comma,        ",",                   82
            whitespace,   "    ",                83
            bracket,      "[",                   87
            bracket,      "]",                   88
            comma,        ",",                   89
            whitespace,   " \n",                 90
            brace,        "{",                   92
            whitespace,   "\n",                  93
            quotedString, "\"NwAssf8pU\"",       94
            colon,        ":",                  105
            null,         "null",               106
            whitespace,   "  ",                 110
            brace,        "}",                  112
            whitespace,   "\n  ",               113
            bracket,      "]",                  116
            */

            InputReader inputReader = new InputReader();
            List<KeyValuePair<ILocation, char>> input = inputReader.InputFromFile(filename);

            // TODO: pass to lexer and compare
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

            /*
            Token categories:

            uppercaseWord: "[A-Z][a-z]*"
            lowercaseWord: "[a-z][a-z]*"
            punctuation:   ".|[,-]"
            space:         " "

            Tokens:

            uppercaseWord, "Velit",         0
            space,         " ",             5
            lowercaseWord, "qui",           6
            space,         " ",             9
            lowercaseWord, "eu",           10
            space,         " ",            12
            lowercaseWord, "cillum",       13
            space,         " ",            19
            lowercaseWord, "anim",         20
            space,         " ",            24
            lowercaseWord, "idunt",       149
            space,         " ",           154
            lowercaseWord, "occaecat",    155
            space,         " ",           163
            lowercaseWord, "ipsum",       164
            space,         " ",           169
            lowercaseWord, "officia",     170
            punctuation,   ".",           177
            space,         " ",           178
            uppercaseWord, "Consectetur", 179
            space,         " ",           190
            lowercaseWord, "labore",      191
            space,         " ",           197
            lowercaseWord, "volup",       198
            space,         " ",           203
            lowercaseWord, "didunt",      226
            punctuation,   ",",           232
            space,         " ",           233
            lowercaseWord, "eu",          234
            space,         " ",           236
            lowercaseWord, "occaecat",    237
            punctuation,   ".",           245
            */

            InputReader inputReader = new InputReader();
            Preprocessor preprocessor = new Preprocessor();
            IEnumerable<KeyValuePair<ILocation, char>> input = inputReader.InputFromString(inputString);
            input = preprocessor.PreprocessInput(input);

            // TODO: pass to lexer and compare
        }
    }
}
