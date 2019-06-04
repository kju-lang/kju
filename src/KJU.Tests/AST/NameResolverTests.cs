namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.Types;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable SA1118  // Parameter must not span multiple lines
    [TestClass]
    public class NameResolverTests
    {
        private readonly IPhase nameResolver = new NameResolver();

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
                var functionCall = new FunctionCall(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "b",
                    new List<Expression>());
                var body = new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression> { functionCall });
                var fun = new FunctionDeclaration(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    id,
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    body,
                    false);
                calls.Add(functionCall);
                functions.Add(fun);
            }

            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
            var resolver = new NameResolver();
            resolver.Run(root, null);
            var bDeclaration = functions[1];
            var expected = new List<FunctionDeclaration> { bDeclaration };
            CollectionAssert.AreEqual(expected, calls[0].DeclarationCandidates);
            CollectionAssert.AreEqual(expected, calls[1].DeclarationCandidates);
            CollectionAssert.AreEqual(expected, calls[2].DeclarationCandidates);
        }

        /*
         * fun a()
         * {
         * }
         * fun b()
         * {
         *   a();
         *   fun a(x: Int)
         *   {
         *   };
         *   a();
         * }
         */
        [TestMethod]
        public void TestShadowing()
        {
            var functions = new List<FunctionDeclaration>();
            var calls = new List<FunctionCall>();

            var body = new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>());
            var fun = new FunctionDeclaration(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "a",
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    body,
                    false);

            var functionCall1 = new FunctionCall(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "a",
                    new List<Expression>());
            calls.Add(functionCall1);

            var inner = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "a",
                UnitType.Instance,
                new List<VariableDeclaration> { new VariableDeclaration(new Range(new StringLocation(0), new StringLocation(1)), IntType.Instance, "x", null) },
                new InstructionBlock(new Range(new StringLocation(0), new StringLocation(1)), new List<Expression>()),
                false);

            var functionCall2 = new FunctionCall(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "a",
                    new List<Expression>());
            calls.Add(functionCall2);

            var fun2 = new FunctionDeclaration(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "b",
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    new InstructionBlock(
                        new Range(new StringLocation(0), new StringLocation(1)),
                        new List<Expression> { functionCall1, inner, functionCall2 }),
                    false);

            functions.Add(fun);
            functions.Add(fun2);

            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
            var resolver = new NameResolver();
            resolver.Run(root, null);
            var expected = new List<FunctionDeclaration> { fun };
            CollectionAssert.AreEqual(expected, calls[0].DeclarationCandidates);
            expected = new List<FunctionDeclaration> { inner };
            CollectionAssert.AreEqual(expected, calls[1].DeclarationCandidates);
        }

        /*
         * fun a()
         * {
         *   unapply(a);
         * }
         */
        [TestMethod]
        public void TestUnapplication()
        {
            var names = new List<string> { "a" };
            var functions = new List<FunctionDeclaration>();
            var unapplications = new List<UnApplication>();
            foreach (var id in names)
            {
                var unapplication = new UnApplication(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "a");
                var body = new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression> { unapplication });
                var fun = new FunctionDeclaration(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    id,
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    body,
                    false);
                unapplications.Add(unapplication);
                functions.Add(fun);
            }

            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
            var resolver = new NameResolver();
            resolver.Run(root, null);
            var aDeclaration = functions[0];
            var expected = new List<FunctionDeclaration> { aDeclaration };
            CollectionAssert.AreEqual(expected, unapplications[0].Candidates.ToList());
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
            var hInstructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression>());
            var h = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "h",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                hInstructionBlock,
                false);
            var x = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "x",
                null);
            var v = new Variable(new Range(new StringLocation(0), new StringLocation(1)), "x");
            var h2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "h",
                null);
            var v3 = new Variable(new Range(new StringLocation(0), new StringLocation(1)), "h");
            var fInstructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { v, h2, v3 });
            var f = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                UnitType.Instance,
                new List<VariableDeclaration> { x },
                fInstructionBlock,
                false);

            var x2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "x",
                null);
            var v2 = new Variable(new Range(new StringLocation(0), new StringLocation(1)), "x");
            var fc = new FunctionCall(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                new List<Expression> { v2 });

            var gInstructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { x2, fc });
            var g = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "g",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                gInstructionBlock,
                false);

            var functions = new List<FunctionDeclaration> { h, f, g };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            var resolver = new NameResolver();
            resolver.Run(root, null);
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
            var x = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "x",
                null);
            var x2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "x",
                null);
            var fInstructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { x, x2 });
            var f = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                fInstructionBlock,
                false);

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var resolver = new NameResolver();
            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(root, diagnostics));
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
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new Range(new StringLocation(0), new StringLocation(1)), new List<Expression>()),
                false);

            var f2 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new Range(new StringLocation(0), new StringLocation(1)), new List<Expression>()),
                false);

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var resolver = new NameResolver();
            var functions = new List<FunctionDeclaration> { f, f2 };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(root, diagnostics));
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
            var gInstructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression>());
            var g = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "g",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                gInstructionBlock,
                false);

            var g2 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "g",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                gInstructionBlock,
                false);

            var instructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { g, g2 });
            var f = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                instructionBlock,
                false);

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(root, diagnostics));
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
            var v = new Variable(new Range(new StringLocation(0), new StringLocation(1)), "x");
            var x = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                UnitType.Instance,
                "x",
                null);

            var fc = new FunctionCall(
                new Range(new StringLocation(0), new StringLocation(1)),
                "g",
                new List<Expression>());

            var instructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { v, x, fc });
            var f = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                instructionBlock,
                false);

            var mockDiagnostics = new Moq.Mock<IDiagnostics>();
            var diagnostics = mockDiagnostics.Object;

            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(root, diagnostics));
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
            var v = new Variable(new Range(new StringLocation(0), new StringLocation(1)), "f");

            var x = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "f",
                null);

            var fc = new FunctionCall(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                new List<Expression>());

            var instructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { v, x, fc });
            var f = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                instructionBlock,
                false);

            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var functions = new List<FunctionDeclaration> { f };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(root, diagnostics));
            MockDiagnostics.Verify(
                diagnosticsMock,
                NameResolver.IdentifierNotFoundDiagnostic);
        }

        /*
         * fun kju() : Unit {
         *   var x : A;
         * }
         *
         * struct A {
         *   A a;
         * }
         */
        [TestMethod]
        public void TestStruct()
        {
            var x = new VariableDeclaration(
                null,
                new UnresolvedType("A", null),
                "x",
                null);
            var kju = new FunctionDeclaration(
                null,
                "kju",
                UnitType.Instance,
                new List<VariableDeclaration>() { },
                new InstructionBlock(
                    null,
                    new List<Expression>()
                    {
                        x,
                    }),
                false);
            var globalADecl = new StructDeclaration(
                null,
                "A",
                new List<StructField>()
                {
                    new StructField(null, "a", new UnresolvedType("A", null))
                });

            var program = new Program(
                null,
                new List<StructDeclaration>() { globalADecl },
                new List<FunctionDeclaration>() { kju });
            this.nameResolver.Run(program, null);

            Assert.AreEqual(StructType.GetInstance(globalADecl), x.VariableType);
            Assert.AreEqual(StructType.GetInstance(globalADecl), globalADecl.Fields[0].Type);
        }

        /*
         *
         * struct A {
         *   int a;
         * }
         * fun kju() : Unit {
         *   var x1 : A;
         *   {
         *     struct A {
         *       int b;
         *     };
         *     var y : A;
         *   };
         *   var x2 : A;
         * }
         */
        [TestMethod]
        public void TestStructShadowing()
        {
            var innerADecl = new StructDeclaration(
                null,
                "A",
                new List<StructField>() { new StructField(null, "b", IntType.Instance) });
            var globalADecl = new StructDeclaration(
                null,
                "A",
                new List<StructField>() { new StructField(null, "a", IntType.Instance) });

            var x1 = new VariableDeclaration(
                null,
                new UnresolvedType("A", null),
                "x1",
                null);
            var x2 = new VariableDeclaration(
                null,
                new UnresolvedType("A", null),
                "x2",
                null);
            var y = new VariableDeclaration(
                null,
                new UnresolvedType("A", null),
                "y",
                null);

            var kju = new FunctionDeclaration(
                null,
                "kju",
                UnitType.Instance,
                new List<VariableDeclaration>() { },
                new InstructionBlock(
                    null,
                    new List<Expression>()
                    {
                        x1,
                        new InstructionBlock(null, new List<Expression>()
                        {
                            innerADecl,
                            y,
                        }),
                        x2,
                    }),
                false);

            var program = new Program(
                null,
                new List<StructDeclaration>() { globalADecl },
                new List<FunctionDeclaration>() { kju });
            this.nameResolver.Run(program, null);

            Assert.AreEqual(StructType.GetInstance(globalADecl), x1.VariableType);
            Assert.AreEqual(StructType.GetInstance(globalADecl), x2.VariableType);
            Assert.AreEqual(StructType.GetInstance(innerADecl), y.VariableType);
        }

        /*
         * struct A {
         *   int x;
         *   bool x;
         * }
         */
        [TestMethod]
        public void TestStructSameFieldName()
        {
            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var declA = new StructDeclaration(
                null,
                "A",
                new List<StructField>()
                {
                    new StructField(null, "x", IntType.Instance),
                    new StructField(null, "x", BoolType.Instance),
                });

            var program = new Program(
                null,
                new List<StructDeclaration>() { declA },
                new List<FunctionDeclaration>() { });
            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(program, diagnostics));

            MockDiagnostics.Verify(
                diagnosticsMock,
                NameResolver.MultipleDeclarationsDiagnostic);
        }

        /*
         * struct A {
         * }
         * struct A {
         * }
         */
        [TestMethod]
        public void TestStructMultipleDeclarations()
        {
            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var aDecl1 = new StructDeclaration(
                null,
                "A",
                new List<StructField>() { });
            var aDecl2 = new StructDeclaration(
                null,
                "A",
                new List<StructField>() { });

            var program = new Program(
                null,
                new List<StructDeclaration>() { aDecl1, aDecl2 },
                new List<FunctionDeclaration>() { });
            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(program, diagnostics));

            MockDiagnostics.Verify(
                diagnosticsMock,
                NameResolver.MultipleDeclarationsDiagnostic,
                NameResolver.MultipleDeclarationsDiagnostic);
        }

        /*
         * struct Int {
         * }
         */
        [TestMethod]
        public void TestStructWithBuilinTypeName()
        {
            var diagnosticsMock = new Moq.Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            var intDecl1 = new StructDeclaration(
                null,
                "Int",
                new List<StructField>() { });
            var program = new Program(
                null,
                new List<StructDeclaration>() { intDecl1 },
                new List<FunctionDeclaration>() { });
            Assert.ThrowsException<NameResolverException>(() => this.nameResolver.Run(program, diagnostics));

            MockDiagnostics.Verify(
                diagnosticsMock,
                NameResolver.TypeIdentifierErrorDiagnosticsType);
        }

        /*
         * struct A {}
         *
         * fun f(a : A) : A {
         *   f(a);
         *   return a;
         * }
         */
        [TestMethod]
        public void TestStructFunctionParameters()
        {
            var aDecl = new StructDeclaration(
                null,
                "A",
                new List<StructField>());

            var aParam = new VariableDeclaration(
                null,
                new UnresolvedType("A", null),
                "a",
                null);
            var f = new FunctionDeclaration(
                null,
                "f",
                new UnresolvedType("A", null),
                new List<VariableDeclaration>() { aParam },
                new InstructionBlock(
                    null,
                    new List<Expression>() {
                        new FunctionCall(
                            null,
                            "f",
                            new List<Expression>() { aParam }),
                        new ReturnStatement(
                            null,
                            new Variable(
                                null,
                                "a")),
                    }),
                false);

            var program = new Program(
                null,
                new List<StructDeclaration>() { aDecl },
                new List<FunctionDeclaration>() { f });

            this.nameResolver.Run(program, null);

            Assert.AreEqual(StructType.GetInstance(aDecl), f.ReturnType);
            Assert.AreEqual(StructType.GetInstance(aDecl), aParam.VariableType);
        }

        /*
         * struct A {}
         * fun kju(): Unit {
         *   new (A);
         * }
         */
        [TestMethod]
        public void TestStructAlloc()
        {
            var aDecl = new StructDeclaration(
                null,
                "A",
                new List<StructField>());
            var aAlloc = new StructAlloc(
                null,
                new UnresolvedType("A", null));
            var kju = new FunctionDeclaration(
                null,
                "kju",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    null,
                    new List<Expression>() {
                        aAlloc
                    }),
                    false);

            var program = new Program(
                null,
                new List<StructDeclaration>() { aDecl },
                new List<FunctionDeclaration>() { kju });

            this.nameResolver.Run(program, null);

            Assert.AreEqual(StructType.GetInstance(aDecl), aAlloc.AllocType);
        }
    }
}