namespace KJU.Tests.Integration
{
    using System.Linq;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class KjuLexerTests
    {
        [TestMethod]
        public void TestSimple()
        {
            var tokens = KjuCompilerUtils.Tokenize("fun hello() { return\n1 + 2; }", null).ToList();
            Assert.AreEqual(
                "Fun,Whitespace,VariableFunctionIdentifier,LParen,RParen,Whitespace,LBrace,Whitespace,Return,Whitespace,DecimalLiteral,Whitespace,Plus,Whitespace,DecimalLiteral,Semicolon,Whitespace,RBrace,Eof",
                string.Join(",", tokens.Select(token => token.Category)));
            Assert.AreEqual(
                "fun, ,hello,(,), ,{, ,return,\n,1, ,+, ,2,;, ,},",
                string.Join(",", tokens.Select(token => token.Text)));
        }
    }
}
