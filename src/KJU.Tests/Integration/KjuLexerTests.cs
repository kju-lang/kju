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
            Console.WriteLine("scanning...");
            Console.WriteLine(KjuLexer.Scan("fun hello() { return 1 + 2; }"));
            Assert.IsFalse(true);
        }
    }
}