namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Core.Diagnostics;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.TypeChecker;
    using KJU.Core.AST.Types;
    using KJU.Core.Input;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;
    using Range = KJU.Core.Lexer.Range;

#pragma warning disable SA1118  // Parameter must not span multiple lines
    [TestClass]
    public class TypeCheckerTests
    {
        private readonly TypeCheckerHelper helper = new TypeCheckerHelper();
        private readonly IPhase typeChecker = new TypeChecker();

        [TestMethod]
        public void IncorrectNumberOfArguments()
        {
            var arg1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Arg1",
                null);

            var var1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var1",
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5));

            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun1",
                UnitType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                        { new ReturnStatement(new Range(new StringLocation(0), new StringLocation(1)), null) }),
                false);

            var fun2 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun2",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var1,
                        new Assignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "Var1")
                            {
                                Declaration = var1
                            },
                            new FunctionCall(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "Fun1",
                                new List<Expression>())
                            {
                                DeclarationCandidates = new List<FunctionDeclaration> { fun1 }
                            })
                    }),
                false);

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));
            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.FunctionOverloadNotFoundDiagnostic,
                TypeChecker.IncorrectAssigmentTypeDiagnostic);
        }

        [TestMethod]
        public void IncorrectTypeOperation()
        {
            var var1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                BoolType.Instance,
                "Var1",
                null);

            var var2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var2",
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5));

            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun1",
                BoolType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var1,
                        var2,
                        new CompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "Var1")
                                { Declaration = var1 },
                            ArithmeticOperationType.Addition,
                            new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), false)),
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Comparison(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "Var1")
                                    { Declaration = var1 },
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "Var2")
                                    { Declaration = var2 },
                                ComparisonType.Equal))
                    }),
                false);

            var functions = new List<FunctionDeclaration> { fun1 };
            Node root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));
            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.IncorrectLeftSideTypeDiagnostic,
                TypeChecker.IncorrectRightSideTypeDiagnostic,
                TypeChecker.IncorrectComparisonTypeDiagnostic);
        }

        [TestMethod]
        public void UnaryOperations()
        {
            // fun fun1:Unit{
            // var var1 : Int = 3;
            // var var2 : Bool = false;
            // +var1;
            // -var1;
            // !var2;
            // }
            var expectedVar1 =
                new VariableDeclaration(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    IntType.Instance,
                    "var1",
                    new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3)
                        { Type = IntType.Instance })
                {
                    Type = UnitType.Instance
                };

            var expectedVar2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                BoolType.Instance,
                "var2",
                new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), false)
                    { Type = BoolType.Instance })
            {
                Type = UnitType.Instance
            };

            var expectedFun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "fun1",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        expectedVar1,
                        expectedVar2,
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Plus,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var1")
                                { Declaration = expectedVar1, Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        },
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Minus,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var1")
                                { Declaration = expectedVar1, Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        },
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Not,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var2")
                                { Declaration = expectedVar2, Type = BoolType.Instance })
                        {
                            Type = BoolType.Instance
                        }
                    }) { Type = UnitType.Instance },
                false) { Type = UnitType.Instance };

            var expectedFunctions = new List<FunctionDeclaration> { expectedFun1 };
            Node expectedRoot = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                expectedFunctions);

            var var1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "var1",
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3));

            var var2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                BoolType.Instance,
                "var2",
                new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), false));

            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "fun1",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var1,
                        var2,
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Plus,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var1")
                                { Declaration = var1 }),
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Minus,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var1")
                                { Declaration = var1 }),
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Not,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var2")
                                { Declaration = var2 }),
                    }),
                false);

            var functions = new List<FunctionDeclaration> { fun1 };
            Node root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            this.typeChecker.Run(root, diagnostics);
            Assert.IsTrue(this.helper.TypeCompareAst(expectedRoot, root));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        [TestMethod]
        public void UnaryOperationsWrongType()
        {
            // fun fun1:Unit{
            // var var1 : Int = 3;
            // var var2 : Bool = false;
            // +var2;
            // -var2;
            // !var1;
            // }
            var var1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "var1",
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3));

            var var2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                BoolType.Instance,
                "var2",
                new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), false));
            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "fun1",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var1,
                        var2,
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Plus,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var2")
                                { Declaration = var2 }),
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Minus,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var2")
                                { Declaration = var2 }),
                        new UnaryOperation(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            UnaryOperationType.Not,
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "var1")
                                { Declaration = var1 }),
                    }),
                false);

            var functions = new List<FunctionDeclaration> { fun1 };
            Node root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));
            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.IncorrectUnaryExpressionTypeDiagnostic,
                TypeChecker.IncorrectUnaryExpressionTypeDiagnostic,
                TypeChecker.IncorrectUnaryExpressionTypeDiagnostic);
        }

        [TestMethod]
        public void SimpleCorrectTyping()
        {
            var root = this.helper.JsonToAst(File.ReadAllText("../../../AST/SimpleAst.json"));
            var expectedRoot = this.helper.JsonToAst(File.ReadAllText("../../../AST/SimpleAstTyped.json"));
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            this.typeChecker.Run(root, diagnostics);
            Assert.IsTrue(this.helper.TypeCompareAst(expectedRoot, root));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        [TestMethod]
        public void CorrectTyping()
        {
            var root = GenUntypedAst();
            var expectedRoot = GenCorrectlyTypedAst();
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            this.typeChecker.Run(root, diagnostics);
            Assert.IsTrue(this.helper.TypeCompareAst(expectedRoot, root));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        [TestMethod]
        public void DetectErrors()
        {
            var root = GenWrongAst();
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));
            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.FunctionOverloadNotFoundDiagnostic,
                TypeChecker.AssignedValueHasNoTypeDiagnostic,
                TypeChecker.IncorrectOperandTypeDiagnostic,
                TypeChecker.IncorrectReturnTypeDiagnostic);
        }

        [TestMethod]
        public void ArrayCorrect()
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            diagnosticsMock.Setup(foo => foo.Add(It.IsAny<Diagnostic[]>()))
                .Throws(new Exception("Diagnostics not empty."));
            var diagnostics = diagnosticsMock.Object;

            /*
             * function f(p : [int]) : [int] {
             *    p[2] = 3;
             *    p[1] += 2;
             *    return p;
             * }
             *
             * function kju() : Unit {
             *    var t : [int] = new (int, 5);
             *    f(t)[4];
             * }
             *
             */

            var t = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                ArrayType.GetInstance(IntType.Instance),
                "t",
                new ArrayAlloc(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    IntType.Instance,
                    new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5)));
            var p = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                ArrayType.GetInstance(IntType.Instance),
                "a",
                null);

            var f = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                ArrayType.GetInstance(IntType.Instance),
                new List<VariableDeclaration>
                {
                    p
                },
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        new ComplexAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "p")
                                    { Declaration = p, Type = ArrayType.GetInstance(IntType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 2)),
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3)),
                        new ComplexCompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "p")
                                    { Declaration = p, Type = ArrayType.GetInstance(IntType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 1)),
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 2)),
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "p")
                                { Declaration = p, Type = ArrayType.GetInstance(IntType.Instance) })
                    }),
                false);

            var kju = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "kju",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        t,
                        new ArrayAccess(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new FunctionCall(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "f",
                                new List<Expression>
                                {
                                    new Variable(new Range(new StringLocation(0), new StringLocation(1)), "t")
                                    {
                                        Declaration = t,
                                        Type = ArrayType.GetInstance(IntType.Instance),
                                    },
                                }) { DeclarationCandidates = new List<FunctionDeclaration>() { f } },
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 4)),
                    }),
                false);

            var functions = new List<FunctionDeclaration> { f, kju };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            this.typeChecker.Run(root, diagnostics);
        }

        [TestMethod]
        public void ArrayAccessResult()
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            diagnosticsMock.Setup(foo => foo.Add(It.IsAny<Diagnostic[]>()))
                .Throws(new Exception("Diagnostics not empty."));
            var diagnostics = diagnosticsMock.Object;

            //  var x: Bool = a[0]. // where a is [bool]

            var a = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                ArrayType.GetInstance(BoolType.Instance),
                "a",
                null);

            var kju = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "kju",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        new VariableDeclaration(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            BoolType.Instance,
                            "x",
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "a")
                                    { Declaration = a, Type = ArrayType.GetInstance(BoolType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 0))),
                    }),
                false);
            var functions = new List<FunctionDeclaration> { kju };
            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            this.typeChecker.Run(root, diagnostics);
        }

        [TestMethod]
        public void ArrayErrors()
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            /*
             * function f() : Unit {
             *     a[2]; // and a is int
             *     b[true];
             *     c[1] = true; // a is [int]
             *     d[1] += 1; // a is [bool]
             *     new (int, true);
             * }
             */

            var a = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "a",
                null);
            var b = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                ArrayType.GetInstance(IntType.Instance),
                "b",
                null);
            var c = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                ArrayType.GetInstance(IntType.Instance),
                "c",
                null);
            var d = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                ArrayType.GetInstance(BoolType.Instance),
                "d",
                null);

            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "kju",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        new ArrayAccess(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "a")
                                { Declaration = a, Type = ArrayType.GetInstance(IntType.Instance) },
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 2)),
                        new ArrayAccess(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "b")
                                { Declaration = b, Type = ArrayType.GetInstance(IntType.Instance) },
                            new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), true)),
                        new ComplexAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "c")
                                    { Declaration = c, Type = ArrayType.GetInstance(IntType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 1)),
                            new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), true)),
                        new ComplexCompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "d")
                                    { Declaration = d, Type = ArrayType.GetInstance(BoolType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 1)),
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 1)),
                        new ArrayAlloc(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            IntType.Instance,
                            new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), true)),
                    }),
                false);

            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                new List<FunctionDeclaration>() { fun1 });

            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));

            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.IncorrectArrayAccessUseDiagnostic,
                TypeChecker.IncorrectArrayIndexTypeDiagnostic,
                TypeChecker.IncorrectArrayTypeDiagnostic,
                TypeChecker.IncorrectArrayTypeDiagnostic,
                TypeChecker.IncorrectArraySizeTypeDiagnostic);
        }

        private static Node GenUntypedAst()
        {
            // int Fun1(Arg1:Int):Int
            // {
            //     var Var1:Int = 5;
            //     return Arg1 + Var1;
            // }

            // int Fun2():Int
            // {
            //     var Var2:Int = Fun1(5);
            //     Var2 += 3;
            //     return Var2;
            // }
            var arg1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Arg1",
                null);

            var var1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var1",
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5));

            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var1,
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArithmeticOperation(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(
                                    new Range(new StringLocation(0), new StringLocation(1)),
                                    "Arg1")
                                {
                                    Declaration = arg1
                                },
                                new Variable(
                                    new Range(new StringLocation(0), new StringLocation(1)),
                                    "Var1")
                                {
                                    Declaration = var1
                                },
                                ArithmeticOperationType.Addition))
                    }),
                false);

            var var2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var2",
                new FunctionCall(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "Fun1",
                    new List<Expression>
                    {
                        new IntegerLiteral(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            5)
                    })
                {
                    DeclarationCandidates = new List<FunctionDeclaration> { fun1 }
                });

            var fun2 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun2",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var2,
                        new CompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "var2")
                            {
                                Declaration = var2
                            },
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3)),
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "var2")
                            {
                                Declaration = var2
                            })
                    }),
                false);

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
        }

        private static Node GenCorrectlyTypedAst()
        {
            // int Fun1(int Arg1)
            // {
            //     int Var1 = 5;
            //     return Arg1 + Var1;
            // }

            // int Fun2()
            // {
            //     int Var2 = Fun1(5);
            //     Var2 += 3;
            //     return Var2;
            // }
            var arg1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Arg1",
                null)
            {
                Type = UnitType.Instance
            };

            var var1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var1",
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5)
                    { Type = IntType.Instance })
            {
                Type = UnitType.Instance
            };

            FunctionDeclaration fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var1,
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArithmeticOperation(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(
                                    new Range(new StringLocation(0), new StringLocation(1)),
                                    "Arg1")
                                {
                                    Declaration = arg1,
                                    Type = IntType.Instance
                                },
                                new Variable(
                                    new Range(new StringLocation(0), new StringLocation(1)),
                                    "Var1")
                                {
                                    Declaration = var1,
                                    Type = IntType.Instance
                                },
                                ArithmeticOperationType.Addition) { Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        }
                    })
                {
                    Type = UnitType.Instance
                },
                false)
            {
                Type = UnitType.Instance
            };

            var var2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var2",
                new FunctionCall(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "Fun1",
                    new List<Expression>
                    {
                        new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5)
                            { Type = IntType.Instance }
                    })
                {
                    DeclarationCandidates = new List<FunctionDeclaration> { fun1 },
                    Declaration = fun1,
                    Type = IntType.Instance
                })
            {
                Type = UnitType.Instance
            };

            var fun2 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun2",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var2,
                        new CompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "var2") { Declaration = var2, Type = IntType.Instance },
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3)
                                { Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        },
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "var2") { Declaration = var2, Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        }
                    })
                {
                    Type = UnitType.Instance
                },
                false)
            {
                Type = UnitType.Instance
            };

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
        }

        private static Node GenWrongAst()
        {
            // int Fun1(Arg1:Bool):Int
            // {
            //     var Var1:Int = 5;
            //     return Arg1 + Var1;
            // }

            // unit Fun2()
            // {
            //     var Var2:Int = Fun1(5);
            //     Var2 += 3;
            //     return Var2;
            // }
            var arg1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                BoolType.Instance,
                "Arg1",
                null);

            var var1 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var1",
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5));

            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var1,
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArithmeticOperation(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "Arg1")
                                {
                                    Declaration = arg1
                                },
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "Var1")
                                {
                                    Declaration = var1
                                },
                                ArithmeticOperationType.Addition))
                    }),
                false);

            var var2 = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                "Var2",
                new FunctionCall(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    "Fun1",
                    new List<Expression>
                        { new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5) })
                {
                    DeclarationCandidates = new List<FunctionDeclaration> { fun1 }
                });

            var fun2 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun2",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        var2,
                        new CompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "var2") { Declaration = var2 },
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3)),
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                "var2") { Declaration = var2 })
                    }),
                false);

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
        }

        private static Node SimpleAst()
        {
            // aka SimpleAst.json && SimpleAstTyped.json
            var fun1 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5))
                    }),
                false);

            var fun2 = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "Fun2",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    new List<Expression>
                    {
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArithmeticOperation(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new FunctionCall(
                                    new Range(new StringLocation(0), new StringLocation(1)),
                                    "Fun1",
                                    new List<Expression>())
                                {
                                    DeclarationCandidates = new List<FunctionDeclaration> { fun1 }
                                },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5),
                                ArithmeticOperationType.Addition))
                    }),
                false);

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);
        }
    }
}