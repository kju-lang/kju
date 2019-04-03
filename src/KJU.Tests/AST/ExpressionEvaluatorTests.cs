namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Diagnostics;
    using KJU.Core.AST;
    using KJU.Core.Parser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;
    using static KJU.Core.AST.ReturnChecker.ExpressionEvaluator;

    [TestClass]
    public class ExpressionEvaluatorTests
    {
        public static IEnumerable<object[]> TestCasesInteger { get; } = new List<object[]>
        {
            new object[] { "-5", -5L },
            new object[] { "+x", (long?)null },
            new object[] { "+(+5)", 5L },
            new object[] { "-x", (long?)null },
            new object[] { "+5", 5L },
            new object[] { "1+2", 3L },
            new object[] { "2*3", 6L },
            new object[] { "2*x", (long?)null },
            new object[] { "9223372036854775807+1", -9223372036854775808L },
            new object[] { "2+2*2", 6L },
            new object[] { "(2+2)*2", 8L },
            new object[] { "101/5", 20L },
            new object[] { "101%5", 1L },
            new object[] { "1+false", (long?)null },
            new object[] { "false", (long?)null },
            new object[] { "1==x", (long?)null }
        }.AsEnumerable();

        public static IEnumerable<object[]> TestCasesBoolean { get; } = new List<object[]>
        {
            new object[] { "1==1", true },
            new object[] { "1==2", false },
            new object[] { "1==2 || true", true },
            new object[] { "1==2 || false", false },
            new object[] { "true || x", true },
            new object[] { "false || x", (bool?)null },
            new object[] { "false && x", false },
            new object[] { "true && x", (bool?)null },
            new object[] { "x || true", true },
            new object[] { "x && false", false },
            new object[] { "(1==2) || true", true },
            new object[] { "1<2", true },
            new object[] { "1<1", false },
            new object[] { "1<-2", false },
            new object[] { "1<2", true },
            new object[] { "1<=1", true },
            new object[] { "1>1", false },
            new object[] { "1>=1", true },
            new object[] { "1==(1+x)", (bool?)null },
            new object[] { "5>=1", true },
            new object[] { "5!=1", true },
            new object[] { "-1!=-1", false },
            new object[] { "!true", false },
            new object[] { "!x", (bool?)null },
            new object[] { "!(1==1)", false },
            new object[] { "1+false", (bool?)null }
        }.AsEnumerable();

        [DataTestMethod]
        [DynamicData(nameof(TestCasesInteger), DynamicDataSourceType.Property)]
        public void AssertEvaluatesInteger(string expression, long? expected)
        {
            Assert.AreEqual(expected, CompileExpression(expression).AsInteger());
        }

        [TestMethod]
        [DataTestMethod]
        [DynamicData(nameof(TestCasesBoolean), DynamicDataSourceType.Property)]
        public void AssertEvaluatesBoolean(string expression, bool? expected)
        {
            Assert.AreEqual(expected, CompileExpression(expression).AsBool());
        }

        private static Expression CompileExpression(string data)
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            var tree = KjuParserFactory.Instance.Parse($"fun kju(): Unit {{ return {data}; }}", diagnostics);
            var ast = new KjuParseTreeToAstConverter().GenerateAst(tree, diagnostics);
            return ((ReturnStatement)((Program)ast).Functions[0].Body.Instructions[0]).Value;
        }
    }
}