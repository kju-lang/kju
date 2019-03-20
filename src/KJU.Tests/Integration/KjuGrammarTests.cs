namespace KJU.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class KjuGrammarTests
    {
        [TestMethod]
        public void TestTable()
        {
            ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);
        }
    }
}