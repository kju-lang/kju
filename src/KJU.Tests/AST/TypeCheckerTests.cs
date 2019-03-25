namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;

#pragma warning disable SA1118  // Parameter must not span multiple lines
    [TestClass]
    public class TypeCheckerTests
    {
        private readonly TypeCheckerHelper helper = new TypeCheckerHelper();

        [TestMethod]
        public void IncorrectNumberOfArguments()
        {
            var arg1 = new VariableDeclaration(IntType.Instance, "Arg1", null);

            var var1 = new VariableDeclaration(IntType.Instance, "Var1", new IntegerLiteral(5));

            var fun1 = new FunctionDeclaration(
                "Fun1",
                UnitType.Instance,
                new List<VariableDeclaration>() { arg1 },
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
                            new Variable(null)
                            {
                                Declaration = var1
                            },
                            new FunctionCall(null, new List<Expression>())
                            {
                                Declaration = fun1
                            })
                    }));

            var functions = new List<FunctionDeclaration> { fun1, fun2 };
            Node root = new Program(functions);
            Diagnostics diags = new Diagnostics();
            TypeChecker tc = new TypeChecker();
            tc.LinkTypes(root, diags);

            Assert.IsTrue(diags.Diags.Any(diag => diag.Message.Contains("Incorrect number of function arguments")
                                                  && diag.Type == TypeChecker.IncorrectNumberOfArgumentsDiagnostic));
            Assert.IsTrue(diags.Diags.Any(diag => diag.Message.Contains("Incorrect assignment value type")
                                                  && diag.Type == TypeChecker.IncorrectTypeDiagnostic));
            Assert.AreEqual(2, diags.Diags.Count());
        }

        [TestMethod]
        public void IncorrectTypeOperation()
        {
            var var1 = new VariableDeclaration(BoolType.Instance, "Var1", null);

            var var2 = new VariableDeclaration(
                IntType.Instance,
                "Var2",
                new IntegerLiteral(5));

            FunctionDeclaration fun1 = new FunctionDeclaration(
                "Fun1",
                BoolType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(
                    new List<Expression>
                    {
                        var1,
                        var2,
                        new CompoundAssignment(
                            new Variable(null) { Declaration = var1 },
                            ArithmeticOperationType.Addition,
                            new BoolLiteral(false)),
                        new ReturnStatement(
                            new Comparison(
                                ComparisonType.Equal,
                                new Variable(null) { Declaration = var1 },
                                new Variable(null) { Declaration = var2 }))
                    }));

            var functions = new List<FunctionDeclaration> { fun1 };
            Node root = new Program(functions);
            Diagnostics diags = new Diagnostics();
            TypeChecker tc = new TypeChecker();
            tc.LinkTypes(root, diags);
            Assert.IsTrue(diags.Diags.Any(diag => diag.Message.Contains("Type mismatch")
                                                  && diag.Type == TypeChecker.IncorrectTypeDiagnostic));
            Assert.IsTrue(diags.Diags.Any(diag => diag.Message.Contains("Incorrect right hand size type")
                                                  && diag.Type == TypeChecker.IncorrectTypeDiagnostic));
            Assert.IsTrue(diags.Diags.Any(diag => diag.Message.Contains("Incorrect left hand size type")
                                                  && diag.Type == TypeChecker.IncorrectTypeDiagnostic));
            Assert.AreEqual(3, diags.Diags.Count());
        }

        [TestMethod]
        public void SimpleCorrectTyping()
        {
            Node root = this.helper.JsonToAst(File.ReadAllText("../../../AST/SimpleAst.json"));
            Node expextedRoot = this.helper.JsonToAst(File.ReadAllText("../../../AST/SimpleAstTyped.json"));
            Diagnostics diags = new Diagnostics();
            TypeChecker tc = new TypeChecker();
            tc.LinkTypes(root, diags);
            Assert.IsTrue(this.helper.TypeCompareAst(expextedRoot, root));
            Assert.IsTrue(diags.Diags.Count == 0);
        }

        [TestMethod]
        public void CorrectTyping()
        {
            Node root = this.GenUntypedAst();
            Node expected = this.GenCorrectlyTypedAst();
            Diagnostics diags = new Diagnostics();
            TypeChecker tc = new TypeChecker();
            tc.LinkTypes(root, diags);
            Assert.IsTrue(this.helper.TypeCompareAst(expected, root));
            Assert.IsTrue(diags.Diags.Count == 0);
        }

        [TestMethod]
        public void DetectErrors()
        {
            Node root = this.GenWrongAst();
            Diagnostics diags = new Diagnostics();
            TypeChecker tc = new TypeChecker();
            tc.LinkTypes(root, diags);
            Assert.IsTrue(diags.Diags.Count == 3);
            foreach (var diag in diags.Diags)
            {
                Assert.IsTrue(diag.Type.Equals(TypeChecker.IncorrectTypeDiagnostic));
                Assert.IsTrue(diag.Status == DiagnosticStatus.Error);
            }
        }

        private Node GenUntypedAst()
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

        private Node GenCorrectlyTypedAst()
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

            FunctionDeclaration fun2 = new FunctionDeclaration(
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

        private Node GenWrongAst()
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

            FunctionDeclaration fun2 = new FunctionDeclaration(
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

        private Node SimpleAst()
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