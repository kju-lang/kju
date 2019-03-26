namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Core.Diagnostics;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;

#pragma warning disable SA1118  // Parameter must not span multiple lines
    [TestClass]
    public class TypeCheckerTests
    {
        private readonly TypeCheckerHelper helper = new TypeCheckerHelper();
        private readonly ITypeChecker typeChecker = new TypeChecker();

        [TestMethod]
        public void IncorrectNumberOfArguments()
        {
            var arg1 = new VariableDeclaration(IntType.Instance, "Arg1", null);

            var var1 = new VariableDeclaration(IntType.Instance, "Var1", new IntegerLiteral(5));

            var fun1 = new FunctionDeclaration(
                "Fun1",
                UnitType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(new List<Expression> { new ReturnStatement(null) }));

            var fun2 = new FunctionDeclaration(
                "Fun2",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        new Assignment(
                            new Variable("Var1")
                            {
                                Declaration = var1
                            },
                            new FunctionCall("Fun1", new List<Expression>())
                            {
                                Declaration = fun1
                            })
                    }));

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            var root = new Program(functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.LinkTypes(root, diagnostics));
            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.IncorrectNumberOfArgumentsDiagnostic,
                TypeChecker.IncorrectAssigmentTypeDiagnostic);
        }

        [TestMethod]
        public void IncorrectTypeOperation()
        {
            var var1 = new VariableDeclaration(BoolType.Instance, "Var1", null);

            var var2 = new VariableDeclaration(
                IntType.Instance,
                "Var2",
                new IntegerLiteral(5));

            var fun1 = new FunctionDeclaration(
                "Fun1",
                BoolType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        var2,
                        new CompoundAssignment(
                            new Variable("Var1") { Declaration = var1 },
                            ArithmeticOperationType.Addition,
                            new BoolLiteral(false)),
                        new ReturnStatement(
                            new Comparison(
                                ComparisonType.Equal,
                                new Variable("Var1") { Declaration = var1 },
                                new Variable("Var2") { Declaration = var2 }))
                    }));

            var functions = new List<FunctionDeclaration> { fun1 };
            Node root = new Program(functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.LinkTypes(root, diagnostics));
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
                new VariableDeclaration(IntType.Instance, "var1", new IntegerLiteral(3) { Type = IntType.Instance })
                {
                    Type = UnitType.Instance
                };

            var expectedVar2 = new VariableDeclaration(
                BoolType.Instance,
                "var2",
                new BoolLiteral(false) { Type = BoolType.Instance })
            {
                Type = UnitType.Instance
            };

            var expectedFun1 = new FunctionDeclaration(
                "fun1",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        expectedVar1,
                        expectedVar2,
                        new UnaryOperation(
                            UnaryOperationType.Plus,
                            new Variable("var1") { Declaration = expectedVar1, Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        },
                        new UnaryOperation(
                            UnaryOperationType.Minus,
                            new Variable("var1") { Declaration = expectedVar1, Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        },
                        new UnaryOperation(
                            UnaryOperationType.Not,
                            new Variable("var2") { Declaration = expectedVar2, Type = BoolType.Instance })
                        {
                            Type = BoolType.Instance
                        }
                    }) { Type = UnitType.Instance }) { Type = UnitType.Instance };

            var expectedFunctions = new List<FunctionDeclaration> { expectedFun1 };
            Node expectedRoot = new Program(expectedFunctions);

            var var1 = new VariableDeclaration(IntType.Instance, "var1", new IntegerLiteral(3));

            var var2 = new VariableDeclaration(
                BoolType.Instance,
                "var2",
                new BoolLiteral(false));

            var fun1 = new FunctionDeclaration(
                "fun1",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        var2,
                        new UnaryOperation(UnaryOperationType.Plus, new Variable("var1") { Declaration = var1 }),
                        new UnaryOperation(UnaryOperationType.Minus, new Variable("var1") { Declaration = var1 }),
                        new UnaryOperation(UnaryOperationType.Not, new Variable("var2") { Declaration = var2 }),
                    }));

            var functions = new List<FunctionDeclaration> { fun1 };
            Node root = new Program(functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            this.typeChecker.LinkTypes(root, diagnostics);
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
            var var1 = new VariableDeclaration(IntType.Instance, "var1", new IntegerLiteral(3));

            var var2 = new VariableDeclaration(
                BoolType.Instance,
                "var2",
                new BoolLiteral(false));
            var fun1 = new FunctionDeclaration(
                "fun1",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        var2,
                        new UnaryOperation(UnaryOperationType.Plus, new Variable("var2") { Declaration = var2 }),
                        new UnaryOperation(UnaryOperationType.Minus, new Variable("var2") { Declaration = var2 }),
                        new UnaryOperation(UnaryOperationType.Not, new Variable("var1") { Declaration = var1 }),
                    }));

            var functions = new List<FunctionDeclaration> { fun1 };
            Node root = new Program(functions);
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.LinkTypes(root, diagnostics));
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
            this.typeChecker.LinkTypes(root, diagnostics);
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
            this.typeChecker.LinkTypes(root, diagnostics);
            Assert.IsTrue(this.helper.TypeCompareAst(expectedRoot, root));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        [TestMethod]
        public void DetectErrors()
        {
            var root = GenWrongAst();
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            Assert.ThrowsException<TypeCheckerException>(() => this.typeChecker.LinkTypes(root, diagnostics));
            MockDiagnostics.Verify(
                diagnosticsMock,
                TypeChecker.IncorrectArgumentTypeDiagnostic,
                TypeChecker.IncorrectOperandTypeDiagnostic,
                TypeChecker.IncorrectReturnTypeDiagnostic);
        }

        private static Node GenUntypedAst()
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
            var arg1 = new VariableDeclaration(IntType.Instance, "Arg1", null);

            var var1 = new VariableDeclaration(
                IntType.Instance,
                "Var1",
                new IntegerLiteral(5));

            var fun1 = new FunctionDeclaration(
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        new ReturnStatement(
                            new ArithmeticOperation(
                                ArithmeticOperationType.Addition,
                                new Variable("Arg1")
                                {
                                    Declaration = arg1
                                },
                                new Variable("Var1")
                                {
                                    Declaration = var1
                                }))
                    }));

            var var2 = new VariableDeclaration(
                IntType.Instance,
                "Var2",
                new FunctionCall(
                    "Fun1",
                    new List<Expression> { new IntegerLiteral(5) })
                {
                    Declaration = fun1
                });

            var fun2 = new FunctionDeclaration(
                "Fun2",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var2,
                        new CompoundAssignment(
                            new Variable("var2")
                            {
                                Declaration = var2
                            },
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(3)),
                        new ReturnStatement(
                            new Variable("var2")
                            {
                                Declaration = var2
                            })
                    }));

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(functions);
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
                IntType.Instance,
                "Arg1",
                null)
            {
                Type = UnitType.Instance
            };

            var var1 = new VariableDeclaration(
                IntType.Instance,
                "Var1",
                new IntegerLiteral(5) { Type = IntType.Instance })
            {
                Type = UnitType.Instance
            };

            FunctionDeclaration fun1 = new FunctionDeclaration(
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        new ReturnStatement(
                            new ArithmeticOperation(
                                ArithmeticOperationType.Addition,
                                new Variable("Arg1")
                                {
                                    Declaration = arg1,
                                    Type = IntType.Instance
                                },
                                new Variable("Var1")
                                {
                                    Declaration = var1,
                                    Type = IntType.Instance
                                }) { Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        }
                    })
                {
                    Type = UnitType.Instance
                })
            {
                Type = UnitType.Instance
            };

            var var2 = new VariableDeclaration(
                IntType.Instance,
                "Var2",
                new FunctionCall(
                    "Fun1",
                    new List<Expression> { new IntegerLiteral(5) { Type = IntType.Instance } })
                {
                    Declaration = fun1,
                    Type = IntType.Instance
                })
            {
                Type = UnitType.Instance
            };

            var fun2 = new FunctionDeclaration(
                "Fun2",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var2,
                        new CompoundAssignment(
                            new Variable("var2") { Declaration = var2, Type = IntType.Instance },
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(3) { Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        },
                        new ReturnStatement(
                            new Variable("var2") { Declaration = var2, Type = IntType.Instance })
                        {
                            Type = IntType.Instance
                        }
                    })
                {
                    Type = UnitType.Instance
                })
            {
                Type = UnitType.Instance
            };

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(functions);
        }

        private static Node GenWrongAst()
        {
            // int Fun1(bool Arg1)
            // {
            //     int Var1 = 5;
            //     return Arg1 + Var1;
            // }

            // unit Fun2()
            // {
            //     int Var2 = Fun1(5);
            //     Var2 += 3;
            //     return Var2;
            // }
            var arg1 = new VariableDeclaration(BoolType.Instance, "Arg1", null);

            var var1 = new VariableDeclaration(
                IntType.Instance,
                "Var1",
                new IntegerLiteral(5));

            var fun1 = new FunctionDeclaration(
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration> { arg1 },
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        new ReturnStatement(
                            new ArithmeticOperation(
                                ArithmeticOperationType.Addition,
                                new Variable("Arg1")
                                {
                                    Declaration = arg1
                                },
                                new Variable("Var1")
                                {
                                    Declaration = var1
                                }))
                    }));

            var var2 = new VariableDeclaration(
                IntType.Instance,
                "Var2",
                new FunctionCall(
                    "Fun1",
                    new List<Expression> { new IntegerLiteral(5) })
                {
                    Declaration = fun1
                });

            var fun2 = new FunctionDeclaration(
                "Fun2",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var2,
                        new CompoundAssignment(
                            new Variable("var2") { Declaration = var2 },
                            ArithmeticOperationType.Addition,
                            new IntegerLiteral(3)),
                        new ReturnStatement(
                            new Variable("var2") { Declaration = var2 })
                    }));

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(functions);
        }

        private static Node SimpleAst()
        {
            // aka SimpleAst.json && SimpleAstTyped.json
            var fun1 = new FunctionDeclaration(
                "Fun1",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        new ReturnStatement(new IntegerLiteral(5))
                    }));

            var fun2 = new FunctionDeclaration(
                "Fun2",
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        new ReturnStatement(
                            new ArithmeticOperation(
                                ArithmeticOperationType.Addition,
                                new FunctionCall(
                                    "Fun1",
                                    new List<Expression>())
                                {
                                    Declaration = fun1
                                },
                                new IntegerLiteral(5)))
                    }));

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            return new Program(functions);
        }
    }
}