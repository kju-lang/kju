namespace KJU.Tests.Integration.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class KjuParserTests
    {
        [TestMethod]
        public void EmptyProgramTest()
        {
            var parser = BuildParser();
            var tree = parser.Parse(new List<Token<KjuAlphabet>> {
                new Token<KjuAlphabet> { Category = KjuAlphabet.Eof },
            });
        }

        [TestMethod]
        public void SimpleProgramTest()
        {
            var parser = BuildParser();
            var tree = parser.Parse(new List<Token<KjuAlphabet>> {
                new Token<KjuAlphabet> { Category = KjuAlphabet.Fun },
                new Token<KjuAlphabet> { Category = KjuAlphabet.VariableFunctionIdentifier, Text = "foo" },
                new Token<KjuAlphabet> { Category = KjuAlphabet.LParen },
                new Token<KjuAlphabet> { Category = KjuAlphabet.RParen },
                new Token<KjuAlphabet> { Category = KjuAlphabet.Colon },
                new Token<KjuAlphabet> { Category = KjuAlphabet.TypeIdentifier },
                new Token<KjuAlphabet> { Category = KjuAlphabet.LBrace },
                new Token<KjuAlphabet> { Category = KjuAlphabet.RBrace },
                new Token<KjuAlphabet> { Category = KjuAlphabet.Eof },
            });
        }

        private static Parser<KjuAlphabet> BuildParser()
        {
            return ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);
        }
    }
}