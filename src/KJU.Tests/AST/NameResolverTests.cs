namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.Diagnostics;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NameResolverTests
    {
        private readonly INameResolver nameResolver = new NameResolver();

        /*
         * fun a()
         * {
         *   b();
         * }
         * fun b()
         * {
         *   b();
         * }
         * fun c()
         * {
         *   b();
         * }
         */
        [TestMethod]
        public void TestNameResolver()
        {
            var names = new List<string> { "a", "b", "c" };
            var functions = new List<FunctionDeclaration>();
            var calls = new List<FunctionCall>();
            foreach (var id in names)
            {
                var functionCall = new FunctionCall("b", new List<Expression>());
                var body = new InstructionBlock(new List<Expression> { functionCall });
                var fun = new FunctionDeclaration(
                    id,
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    body);
                calls.Add(functionCall);
                functions.Add(fun);
            }

            var root = new Program(functions);
            var resolver = new NameResolver();
            resolver.LinkNames(root, null);
            var bDeclaration = functions[1];
            var expected = new List<FunctionDeclaration> { bDeclaration };
            CollectionAssert.AreEqual(expected, calls[0].DeclarationCandidates);
            CollectionAssert.AreEqual(expected, calls[1].DeclarationCandidates);
            CollectionAssert.AreEqual(expected, calls[2].DeclarationCandidates);
        }

        /*
         * fun h()
         * {
         * }
         * fun f(x)
         * {
         *   x;
         *   var h;
         *   h;
         * }
         * fun g()
         * {
         *   var x;
         *   f(x);
         * }
         */
        [TestMethod]
        public void TestNameResolverBigger()
        {
            var hInstructionBlock = new InstructionBlock(new List<Expression>());
            var h = new FunctionDeclaration(
                "h",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                hInstructionBlock);
            var x = new VariableDeclaration(IntType.Instance, "x", null);
            var v = new Variable("x");
            var h2 = new VariableDeclaration(IntType.Instance, "h", null);
            var v3 = new Variable("h");
            var fInstructionBlock = new InstructionBlock(new List<Expression> { v, h2, v3 });
            var f = new FunctionDeclaration(
                "f",
                UnitType.Instance,
                new List<VariableDeclaration> { x },
                fInstructionBlock);

            var x2 = new VariableDeclaration(IntType.Instance, "x", null);
            var v2 = new Variable("x");
            var fc = new FunctionCall("f", new List<Expression> { v2 });

            var gInstructionBlock = new InstructionBlock(new List<Expression> { x2, fc });
            var g = new FunctionDeclaration(
                "g",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                gInstructionBlock);

            var functions = new List<FunctionDeclaration> { h, f, g };
            var root = new Program(functions);

            var resolver = new NameResolver();
            resolver.LinkNames(root, null);
            var expected = new List<Node> { x, h2, x2, f };
            var actual = new List<Node> { v.Declaration, v3.Declaration, v2.Declaration };
            actual.AddRange(fc.DeclarationCandidates);
            CollectionAssert.AreEqual(expected, actual);
        }

        /*
         * fun f():Unit
         * {
         *   var x:Int;
         *   var x:Int;
         * }
         */
        [TestMethod]
        public void TestVariableRedeclaration()
        {
            var x = new VariableDeclaration(IntType.Instance, "x", null);
            var x2 = new VariableDeclaration(IntType.Instance, "x", null);
            var fInstructionBlock = new InstructionBlock(new List<Expression> { x, x2 });
            var f = new FunctionDeclaration(
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                fInstructionBlock);

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var resolver = new NameResolver();
            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.LinkNames(root, diagnostics));
            MockDiagnostics.Verify(diagnosticsMock, NameResolver.MultipleDeclarationsDiagnostic);
        }

        /*
         * fun f():Unit
         * {
         * }
         * fun f():Unit
         * {
         * }
         */
        [TestMethod]
        public void TestFunctionRedeclaration()
        {
            var f = new FunctionDeclaration(
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new List<Expression>()));

            var f2 = new FunctionDeclaration(
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new List<Expression>()));

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var resolver = new NameResolver();
            var functions = new List<FunctionDeclaration> { f, f2 };
            var root = new Program(functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.LinkNames(root, diagnostics));
            MockDiagnostics.Verify(diagnosticsMock, NameResolver.MultipleDeclarationsDiagnostic);
        }

        /*
         * fun f():Unit
         * {
         *   fun g():Unit
         *   {
         *   };
         *   fun g():Unit
         *   {
         *   };
         * }
         */
        [TestMethod]
        public void TestInnerFunctionRedeclaration()
        {
            var gInstructionBlock = new InstructionBlock(new List<Expression>());
            var g = new FunctionDeclaration(
                "g",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                gInstructionBlock);

            var g2 = new FunctionDeclaration(
                "g",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                gInstructionBlock);

            var f = new FunctionDeclaration(
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new List<Expression> { g, g2 }));

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var resolver = new NameResolver();
            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.LinkNames(root, diagnostics));
            MockDiagnostics.Verify(diagnosticsMock, NameResolver.MultipleDeclarationsDiagnostic);
        }

        /*
         * fun f():Unit
         * {
         *   x;
         *   var x:Unit;
         *   g();
         * }
         */
        [TestMethod]
        public void TestNoDeclaration()
        {
            var v = new Variable("x");
            var x = new VariableDeclaration(UnitType.Instance, "x", null);

            var fc = new FunctionCall("g", new List<Expression>());

            var f = new FunctionDeclaration(
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new List<Expression> { v, x, fc }));

            var mockDiagnostics = new Moq.Mock<IDiagnostics>();
            var diagnostics = mockDiagnostics.Object;

            var resolver = new NameResolver();
            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.LinkNames(root, diagnostics));
            MockDiagnostics.Verify(
                mockDiagnostics,
                NameResolver.IdentifierNotFoundDiagnostic,
                NameResolver.IdentifierNotFoundDiagnostic);
        }

        /*
         * fun f():Unit
         * {
         *   f;
         *   var f:Int;
         *   f();
         * }
         */
        [TestMethod]
        public void TestWrongDeclaration()
        {
            var v = new Variable("f");

            var x = new VariableDeclaration(IntType.Instance, "f", null);

            var fc = new FunctionCall("f", new List<Expression>());

            var f = new FunctionDeclaration(
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new List<Expression> { v, x, fc }));

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var resolver = new NameResolver();
            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.LinkNames(root, diagnostics));
            MockDiagnostics.Verify(
               diagnosticsMock,
               NameResolver.IdentifierNotFoundDiagnostic);
        }
    }
}