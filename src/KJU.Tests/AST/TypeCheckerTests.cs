namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
    using StructCandidate = System.Collections.Generic.KeyValuePair<Core.AST.StructDeclaration, Core.AST.StructField>;

#pragma warning disable SA1118  // Parameter must not span multiple lines
    [TestClass]
    public class TypeCheckerTests
    {
        private static readonly Range MockRange = new Range(new StringLocation(0), new StringLocation(1));
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
        [Ignore]
        //After change to type checker, this program should fail later, in Solver.
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
                new ArrayType(IntType.Instance),
                "t",
                new ArrayAlloc(
                    new Range(new StringLocation(0), new StringLocation(1)),
                    IntType.Instance,
                    new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 5)));
            var p = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                new ArrayType(IntType.Instance),
                "a",
                null);

            var f = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "f",
                new ArrayType(IntType.Instance),
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
                                    { Declaration = p, Type = new ArrayType(IntType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 2)),
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 3)),
                        new ComplexCompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "p")
                                    { Declaration = p, Type = new ArrayType(IntType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 1)),
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 2)),
                        new ReturnStatement(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "p")
                                { Declaration = p, Type = new ArrayType(IntType.Instance) })
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
                                        Type = new ArrayType(IntType.Instance),
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
                new ArrayType(BoolType.Instance),
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
                                    { Declaration = a, Type = new ArrayType(BoolType.Instance) },
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
                new ArrayType(IntType.Instance),
                "b",
                null);
            var c = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                new ArrayType(IntType.Instance),
                "c",
                null);
            var d = new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                new ArrayType(BoolType.Instance),
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
                                { Declaration = a, Type = new ArrayType(IntType.Instance) },
                            new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 2)),
                        new ArrayAccess(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new Variable(new Range(new StringLocation(0), new StringLocation(1)), "b")
                                { Declaration = b, Type = new ArrayType(IntType.Instance) },
                            new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), true)),
                        new ComplexAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "c")
                                    { Declaration = c, Type = new ArrayType(IntType.Instance) },
                                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 1)),
                            new BoolLiteral(new Range(new StringLocation(0), new StringLocation(1)), true)),
                        new ComplexCompoundAssignment(
                            new Range(new StringLocation(0), new StringLocation(1)),
                            new ArrayAccess(
                                new Range(new StringLocation(0), new StringLocation(1)),
                                new Variable(new Range(new StringLocation(0), new StringLocation(1)), "d")
                                    { Declaration = d, Type = new ArrayType(BoolType.Instance) },
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
                TypeChecker.IncorrectAssigmentTypeDiagnostic,
                TypeChecker.IncorrectLeftSideTypeDiagnostic,
                TypeChecker.IncorrectArraySizeTypeDiagnostic);
        }

        [TestMethod]
        public void StructUsage()
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            diagnosticsMock.Setup(foo => foo.Add(It.IsAny<Diagnostic[]>())).Throws(new Exception("Diagnostics not empty."));
            var diagnostics = diagnosticsMock.Object;

            /*
              def kju() : Unit {
                struct S {
                  x : Int;
                  y : Int;
                };

                var s : S = new(S);
                var res : Int = 0;

                res = (s.x = 10);
                res = (s.y = 20);
                res = (s.x = s.y);
                res = (s.y += 30);
                res = (s.x += s.y);
              }
             */

            var range = new Range(new StringLocation(-1), new StringLocation(-1));

            var structFields = new List<StructField>() {
                new StructField(range, "x", IntType.Instance),
                new StructField(range, "y", IntType.Instance) };

            var structDeclaration = new StructDeclaration(range, "S", structFields);
            var structType = StructType.GetInstance(structDeclaration);

            var structAlloc = new StructAlloc(range, structType) { Declaration = structDeclaration };

            var structVarDeclaration = new VariableDeclaration(range, structType, "s", structAlloc);
            var resVarDeclaration = new VariableDeclaration(range, IntType.Instance, "res", new IntegerLiteral(range, 0));

            Func<Variable> getStructVar = () => new Variable(range, "s") { Declaration = structVarDeclaration };
            Func<string, StructCandidate> getCandidate =
                field => new StructCandidate(structDeclaration, structFields.Single(f => f.Name == field));

            Func<string, List<StructCandidate>> getCandidates =
                field => new List<StructCandidate>() { getCandidate(field) };

            Func<string, Expression> getFieldAccess =
                field => new FieldAccess(range, getStructVar(), field) { StructCandidates = getCandidates(field) };

            Func<Variable> getResVar = () => new Variable(range, "res") { Declaration = resVarDeclaration };
            Func<Expression, Expression> saveResultInRes = expresssion => new Assignment(range, getResVar(), expresssion);

            var kjuInstructions = new List<Expression> {
                structDeclaration,
                structVarDeclaration,
                resVarDeclaration,
                saveResultInRes(new ComplexAssignment(range, getFieldAccess("x"), new IntegerLiteral(range, 10))),
                saveResultInRes(new ComplexAssignment(range, getFieldAccess("y"), new IntegerLiteral(range, 20))),
                saveResultInRes(new ComplexAssignment(range, getFieldAccess("x"), getFieldAccess("y"))),
                saveResultInRes(new ComplexCompoundAssignment(range, getFieldAccess("y"), ArithmeticOperationType.Addition, new IntegerLiteral(range, 30))),
                saveResultInRes(new ComplexCompoundAssignment(range, getFieldAccess("x"), ArithmeticOperationType.Addition, getFieldAccess("y"))) };

            var kjuDeclaration = new FunctionDeclaration(
                range,
                "kju",
                new ArrayType(UnitType.Instance),
                new List<VariableDeclaration>(),
                new InstructionBlock(range, kjuInstructions),
                false);

            var root = new Program(range, new List<StructDeclaration>(), new List<FunctionDeclaration> { kjuDeclaration });
            this.typeChecker.Run(root, diagnostics);
        }

        [TestMethod]
        public void StructFieldAssignmentErrors()
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            /*
              def kju() : Unit {
                struct S {
                  x : Int;
                  y : Bool;
                };

                var s : S = new(S);

                s.x = true;
                s.y = 1;
                s.x = s.y;

                s.x += s.y;
                s.y += 1;
                s.x.x;
              }
             */

            var range = new Range(new StringLocation(-1), new StringLocation(-1));

            var structFields = new List<StructField>() {
                new StructField(range, "x", IntType.Instance),
                new StructField(range, "y", BoolType.Instance) };

            var structDeclaration = new StructDeclaration(range, "S", structFields);
            var structType = StructType.GetInstance(structDeclaration);

            var structAlloc = new StructAlloc(range, structType) { Declaration = structDeclaration };

            var structVarDeclaration = new VariableDeclaration(range, structType, "s", structAlloc);
            var resVarDeclaration = new VariableDeclaration(range, IntType.Instance, "res", new IntegerLiteral(range, 0));

            Func<Variable> getStructVar = () => new Variable(range, "s") { Declaration = structVarDeclaration };
            Func<string, Expression> getFieldAccess = field => new FieldAccess(range, getStructVar(), field);

            var kjuInstructions = new List<Expression> {
                structDeclaration,
                structVarDeclaration,
                new ComplexAssignment(range, getFieldAccess("x"), new BoolLiteral(range, true)),
                new ComplexAssignment(range, getFieldAccess("y"), new IntegerLiteral(range, 1)),
                new ComplexAssignment(range, getFieldAccess("x"), getFieldAccess("y")),
                new ComplexCompoundAssignment(range, getFieldAccess("x"), ArithmeticOperationType.Addition, getFieldAccess("y")),
                new ComplexCompoundAssignment(range, getFieldAccess("y"), ArithmeticOperationType.Addition, new IntegerLiteral(range, 1)),
                new FieldAccess(range, getFieldAccess("x"), "x"),
                getFieldAccess("z") };

            var kjuDeclaration = new FunctionDeclaration(
                range,
                "kju",
                new ArrayType(UnitType.Instance),
                new List<VariableDeclaration>(),
                new InstructionBlock(range, kjuInstructions),
                false);

            var root = new Program(range, new List<StructDeclaration>(), new List<FunctionDeclaration> { kjuDeclaration });
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));

            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.IncorrectAssigmentTypeDiagnostic,
                TypeChecker.IncorrectAssigmentTypeDiagnostic,
                TypeChecker.IncorrectAssigmentTypeDiagnostic,
                TypeChecker.IncorrectRightSideTypeDiagnostic,
                TypeChecker.IncorrectLeftSideTypeDiagnostic,
                TypeChecker.IncorrectStructTypeDiagnostic,
                TypeChecker.IncorrectFieldNameDiagnostic);
        }

        [TestMethod]
        public void StructToStructAssignmentError()
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;

            /*
              struct A {
                x : Int;
              };

              struct B {
                x : Int;
              };

              def kju() : Unit {
                var s1 : A = new(B);
              }
             */

            var range = new Range(new StringLocation(-1), new StringLocation(-1));

            var structFields = new List<StructField>() { new StructField(range, "x", IntType.Instance) };
            var aStructDeclaration = new StructDeclaration(range, "A", structFields);
            var bStructDeclaration = new StructDeclaration(range, "B", structFields);

            var varDeclaration = new VariableDeclaration(
                range,
                StructType.GetInstance(aStructDeclaration),
                "s",
                new StructAlloc(range, StructType.GetInstance(bStructDeclaration)) { Declaration = bStructDeclaration });

            var kjuDeclaration = new FunctionDeclaration(
                range,
                "kju",
                new ArrayType(UnitType.Instance),
                new List<VariableDeclaration>(),
                new InstructionBlock(range, new List<Expression> { varDeclaration }),
                false);

            var root = new Program(
                range,
                new List<StructDeclaration>() { aStructDeclaration, bStructDeclaration },
                new List<FunctionDeclaration> { kjuDeclaration });

            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));
            MockDiagnostics.Verify(diagnosticsMock, TypeChecker.IncorrectAssigmentTypeDiagnostic);
        }

        [TestMethod]
        public void NullUsage()
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            diagnosticsMock.Setup(foo => foo.Add(It.IsAny<Diagnostic[]>())).Throws(new Exception("Diagnostics not empty."));
            var diagnostics = diagnosticsMock.Object;

            /*
              def kju() : Unit {
                struct S {};

                var x : S = null;
                var y : [Int] = null;

                var res : Bool = false;

                x = null;
                y = null;

                res = (x == null);
                res = (null == x);
                res = (y == null);
                res = (null == y);
                res = (null == null);

                res = (x != null);
                res = (null != x);
                res = (y != null);
                res = (null != y);
                res = (null != null);
              }
             */

            var range = new Range(new StringLocation(-1), new StringLocation(-1));
            Func<Expression> getNull = () => new NullLiteral(range);

            var structDeclaration = new StructDeclaration(range, "S", new List<StructField>());
            var structType = StructType.GetInstance(structDeclaration);

            var structVarDeclaration = new VariableDeclaration(range, structType, "x", getNull());
            var arrayVarDeclaration = new VariableDeclaration(range, new ArrayType(IntType.Instance), "y", getNull());

            var resVarDeclaration = new VariableDeclaration(range, BoolType.Instance, "res", new BoolLiteral(range, false));

            Func<Variable> getStructVar = () => new Variable(range, "x") { Declaration = structVarDeclaration };
            Func<Variable> getArrayVar = () => new Variable(range, "y") { Declaration = arrayVarDeclaration };
            Func<Variable> getResVar = () => new Variable(range, "res") { Declaration = resVarDeclaration };

            Func<Expression, Expression> saveResultInRes = expresssion => new Assignment(range, getResVar(), expresssion);

            var kjuInstructions = new List<Expression> {
                structDeclaration,
                structVarDeclaration,
                arrayVarDeclaration,
                resVarDeclaration,
                new ComplexAssignment(range, getStructVar(), getNull()),
                new ComplexAssignment(range, getArrayVar(), getNull()),
                saveResultInRes(new Comparison(range, getStructVar(), getNull(), ComparisonType.Equal)),
                saveResultInRes(new Comparison(range, getNull(), getStructVar(), ComparisonType.Equal)),
                saveResultInRes(new Comparison(range, getArrayVar(), getNull(), ComparisonType.Equal)),
                saveResultInRes(new Comparison(range, getNull(), getArrayVar(), ComparisonType.Equal)),
                saveResultInRes(new Comparison(range, getNull(), getNull(), ComparisonType.Equal)),
                saveResultInRes(new Comparison(range, getStructVar(), getNull(), ComparisonType.NotEqual)),
                saveResultInRes(new Comparison(range, getNull(), getStructVar(), ComparisonType.NotEqual)),
                saveResultInRes(new Comparison(range, getArrayVar(), getNull(), ComparisonType.NotEqual)),
                saveResultInRes(new Comparison(range, getNull(), getArrayVar(), ComparisonType.NotEqual)),
                saveResultInRes(new Comparison(range, getNull(), getNull(), ComparisonType.NotEqual)) };

            var kjuDeclaration = new FunctionDeclaration(
                range,
                "kju",
                new ArrayType(UnitType.Instance),
                new List<VariableDeclaration>(),
                new InstructionBlock(range, kjuInstructions),
                false);

            var root = new Program(range, new List<StructDeclaration>(), new List<FunctionDeclaration> { kjuDeclaration });
            this.typeChecker.Run(root, diagnostics);
        }

        [TestMethod]
        public void NestedFunctionCorrectType()
        {
            var root = NestedFunctionAst(BoolType.Instance, new BoolLiteral(MockRange, true));
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            this.typeChecker.Run(root, diagnostics);
        }

        [TestMethod]
        public void NestedFunctionIncorrectType()
        {
            var root = NestedFunctionAst(BoolType.Instance, new IntegerLiteral(MockRange, 0));
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnostics));
            MockDiagnostics.Verify(diagnosticsMock, TypeChecker.IncorrectReturnTypeDiagnostic);
        }

        [TestMethod]
        public void SuccessfulApplication()
        {
            // fun f(Bool): Int { };
            // fun g(): Int { };
            // fun g(Bool): Int { };
            //
            // var x: (Bool) -> Int = unapply(f);
            // var x: Int = apply(x, false);
            // x = unapply(g);

            Func<IntegerLiteral> intLiteral = () => new IntegerLiteral(MockRange, 0);
            var functionF = ShortFunction("f", new[] { BoolType.Instance }, IntType.Instance, intLiteral());
            var functionG1 = ShortFunction("g", IntType.Instance, intLiteral());
            var functionG2 = ShortFunction("g", new[] { BoolType.Instance }, IntType.Instance, intLiteral());

            Func<string, FunctionDeclaration[], UnApplication> makeUnapplication =
                (name, candidates) => new UnApplication(MockRange, name) { Candidates = candidates.ToList() };

            var declX = new VariableDeclaration(
                MockRange,
                new FunType(functionF),
                "x",
                makeUnapplication("f", new[] { functionF }));

            var declY = new VariableDeclaration(
                MockRange,
                IntType.Instance,
                "y",
                new Application(
                    MockRange,
                    new Variable(MockRange, "x") { Declaration = declX },
                    new List<Expression> { new BoolLiteral(MockRange, false) }));

            var root = ShortProgram(
                functionF,
                functionG1,
                functionG2,
                declX,
                declY,
                new Assignment(
                    MockRange,
                    new Variable(MockRange, "x") { Declaration = declX },
                    makeUnapplication("g", new[] { functionG1, functionG2 })));

            var diagnosticsMock = new Mock<IDiagnostics>();
            this.typeChecker.Run(root, diagnosticsMock.Object);
        }

        [TestMethod]
        public void UnsuccessfulApplication()
        {
            // fun f(Bool): Int { };
            // fun g(): Int { };
            // fun g(Bool): Int { };
            //
            // var x: (Bool) -> Int = unapply(f);
            // apply(y);
            // apply(x, 0);
            // unapply(g);

            Func<IntegerLiteral> intLiteral = () => new IntegerLiteral(MockRange, 0);
            var functionF = ShortFunction("f", new[] { BoolType.Instance }, IntType.Instance, intLiteral());
            var functionG1 = ShortFunction("g", IntType.Instance, intLiteral());
            var functionG2 = ShortFunction("g", new[] { BoolType.Instance }, IntType.Instance, intLiteral());

            Func<string, FunctionDeclaration[], UnApplication> makeUnapplication =
                (name, candidates) => new UnApplication(MockRange, name) { Candidates = candidates.ToList() };

            var declX = new VariableDeclaration(MockRange, new FunType(functionF), "x", makeUnapplication("f", new[] { functionF }));
            var declY = new VariableDeclaration(MockRange, IntType.Instance, "y", intLiteral());

            var root = ShortProgram(
                functionF,
                functionG1,
                functionG2,
                declX,
                declY,
                new Application(MockRange, new Variable(MockRange, "y") { Declaration = declY }, new List<Expression>()),
                new Application(MockRange, new Variable(MockRange, "x") { Declaration = declX }, new List<Expression> { intLiteral() }),
                makeUnapplication("g", new[] { functionG1, functionG2 }));

            var diagnosticsMock = new Mock<IDiagnostics>();
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.Run(root, diagnosticsMock.Object));
            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.IncorrectApplicationFuncDiagnostic,
                TypeChecker.IncorrectApplicationArgsDiagnostic,
                TypeChecker.AmbiguousUnapplicationDiagnostic);
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

        private static Program NestedFunctionAst(DataType type, Expression value)
        {
            // fun kju(): Int {
            //     fun f(): Int {
            //          fun g(): <type> {
            //              return <value>;
            //          }
            //          return 1;
            //     }
            //     return 0;
            // }

            return ShortProgram(
                ShortFunction(
                    "f",
                    IntType.Instance,
                    new IntegerLiteral(MockRange, 1),
                    ShortFunction("g", type, value)));
        }

        private static Program ShortProgram(params Expression[] instructions)
        {
            var kju = ShortFunction("kju", IntType.Instance, new IntegerLiteral(MockRange, 0), instructions);
            return new Program(MockRange, new List<StructDeclaration>(), new List<FunctionDeclaration> { kju });
        }

        private static FunctionDeclaration ShortFunction(string name, DataType retType, Expression retVal, params Expression[] body)
        {
            return ShortFunction(name, new DataType[0], retType, retVal, body);
        }

        private static FunctionDeclaration ShortFunction(string name, DataType[] paramTypes, DataType retType, Expression retVal, params Expression[] body)
        {
            var instructions = new List<Expression>(body);
            instructions.Add(new ReturnStatement(MockRange, retVal));
            var parameters = paramTypes.Select(
                type => new VariableDeclaration(MockRange, type, "a", null)).ToList();
            return new FunctionDeclaration(MockRange, name, retType, parameters, new InstructionBlock(MockRange, instructions), false);
        }
    }
}