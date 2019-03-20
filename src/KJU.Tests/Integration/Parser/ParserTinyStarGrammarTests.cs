namespace KJU.Tests.Integration.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class ParserTinyStarGrammarTests
    {
        private enum Alphabet
        {
            S,
            X,
            EOF
        }

        [TestMethod]
        public void TestSuccess()
        {
            var parser = ParserFactory<Alphabet>.MakeParser(GetGrammar(), Alphabet.EOF);
            var tokens = new List<Token<Alphabet>>
            {
                new Token<Alphabet> { Category = Alphabet.X },
                new Token<Alphabet> { Category = Alphabet.EOF },
            };

            var tree = parser.Parse(tokens);
        }

        private static Grammar<Alphabet> GetGrammar()
        {
            return new Grammar<Alphabet>
            {
                StartSymbol = Alphabet.S,
                Rules = new ReadOnlyCollection<Rule<Alphabet>>(new List<Rule<Alphabet>>
                {
                    new Rule<Alphabet>
                    {
                        Lhs = Alphabet.S,
                        Rhs = Alphabet.X.ToRegex().Starred()
                    },
                })
            };
        }
    }
}