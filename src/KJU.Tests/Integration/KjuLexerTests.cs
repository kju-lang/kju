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
    public class KjuLexerTests
    {
        [TestMethod]
        public void TestSimple()
        {
            var tokens = KjuLexer.Scan("fun hello() { return\n1 + 2; }").ToList();
            Assert.AreEqual(
                "Fun,VariableFunctionIdentifier,LParen,RParen,LBrace,VariableFunctionIdentifier,DecimalLiteral,Plus,DecimalLiteral,Semicolon,RBrace",
                string.Join(",", tokens.Select(token => token.Category)));
            Assert.AreEqual(
                "fun,hello,(,),{,return,1,+,2,;,}",
                string.Join(",", tokens.Select(token => token.Text)));
        }
    }
}