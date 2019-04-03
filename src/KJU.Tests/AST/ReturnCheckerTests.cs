namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Diagnostics;
    using KJU.Core.AST;
    using KJU.Core.AST.ReturnChecker;
    using KJU.Core.Parser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;

    [TestClass]
    public class ReturnCheckerTests
    {
        public static IEnumerable<object[]> PositiveTestCases { get; } = new List<object[]>
        {
            new object[]
            {
                "fun foo(): Unit {}",
                new string[] { }
            },
            new object[]
            {
                "fun foo(): Int { return 5; }",
                new string[] { }
            },
            new object[]
            {
                "fun foo(): Int { 1+2; return 5; }",
                new string[] { }
            },
            new object[]
            {
                "fun foo(): Int { fun bar(): Unit {  }; return 5; }",
                new string[] { }
            },
            new object[]
            {
                "fun foo(): Int { if true then { return 4; } else { return 5; }; }",
                new string[] { }
            },
            new object[]
            {
                "fun foo(): Int { while x {}; return 5; }",
                new string[] { }
            },
            new object[]
            {
                @"fun foo(): Int {while true { if (false) then  { return 5; } else {}; while true { break; };};}",
                new string[] { }
            }
        };

        public static IEnumerable<object[]> NegativeTestCases { get; } = new List<object[]>
        {
            new object[]
            {
                "fun foo(): Int {}",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { while true {}; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { 1+2; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { return 1; 1+2; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { fun bar(): Int {  }; return 5; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { if true then { 1; } else { return 5; }; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { while true { break; }; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { while 1==(2-1) { }; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { while 1==(1+1) { }; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                "fun foo(): Int { while x {}; }",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                @"fun foo(): Int { var x:Bool=true;  var y:Bool=false;  while (x||y) {return 0;};}",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                @"fun foo(): Int {while true { if (false) then { return 5; } else {}; break; };}",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                @"fun foo(): Int {while true { if (false) then { return 5; } else {}; { break; };};}",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            },
            new object[]
            {
                @"fun foo(): Int {while true { fun bar():Int {return 5;}; };}",
                new[] { ReturnChecker.MissingReturnDiagnostic }
            }
        }.AsEnumerable();

        [DataTestMethod]
        [DynamicData(nameof(PositiveTestCases), DynamicDataSourceType.Property)]
        public void CheckPositive(string data, params string[] diagTypes)
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Compile(data, diagnostics);
            MockDiagnostics.Verify(diagnosticsMock, diagTypes);
        }

        [DataTestMethod]
        [DynamicData(nameof(NegativeTestCases), DynamicDataSourceType.Property)]
        public void CheckNegative(string data, params string[] diagTypes)
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<ReturnCheckerException>(() => Compile(data, diagnostics));
            MockDiagnostics.Verify(diagnosticsMock, diagTypes);
        }

        private static void Compile(string data, IDiagnostics diagnostics)
        {
            var tree = KjuParserFactory.Instance.Parse(data, diagnostics);
            var ast = new KjuParseTreeToAstConverter().GenerateAst(tree, diagnostics);
            new ReturnChecker().Run(ast, diagnostics);
        }
    }
}