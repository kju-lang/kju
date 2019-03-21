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

    [TestClass]
    public class TypeCheckerTests
    {
        private TypeCheckerHelper helper = new TypeCheckerHelper();

        [TestMethod]
        public void IncorrectNumberOfArguments()
        {
            var arg1 = new VariableDeclaration()
            {
                VariableType = IntType.Instance,
                Identifier = "Arg1",
            };

            var var1 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var1",
                Value = new IntegerLiteral() { Value = 5 }
            };

            FunctionDeclaration fun1 = new FunctionDeclaration
            {
                Identifier = "Fun1",
                ReturnType = UnitType.Instance,
                Parameters = new List<VariableDeclaration>() { arg1 },
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression>
                    {
                        new ReturnStatement()
                    }
                }
            };

            FunctionDeclaration fun2 = new FunctionDeclaration
            {
                Identifier = "Fun2",
                ReturnType = UnitType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression>
                    {
                        var1,
                        new Assignment()
                        {
                            Lhs = new Variable { Declaration = var1 },
                            Value = new FunctionCall { Declaration = fun1, Arguments = new List<Expression>() }
                        }
                    }
                }
            };

            Node root = new Program { Functions = new List<FunctionDeclaration> { fun1, fun2 } };
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
            var var1 = new VariableDeclaration()
            {
                VariableType = BoolType.Instance,
                Identifier = "Var1",
            };

            var var2 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var2",
                Value = new IntegerLiteral() { Value = 5 }
            };

            FunctionDeclaration fun1 = new FunctionDeclaration
            {
                Identifier = "Fun1",
                ReturnType = BoolType.Instance,
                Parameters = new List<VariableDeclaration>() { },
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression>
                    {
                        var1,
                        var2,
                        new CompoundAssignment
                        {
                            Lhs = new Variable { Declaration = var1 },
                            Operation = ArithmeticOperationType.Addition,
                            Value = new BoolLiteral { }
                        },
                        new ReturnStatement
                        {
                            Value = new Comparison {
                                LeftValue = new Variable { Declaration = var1 },
                                RightValue = new Variable { Declaration = var2 }
                            }
                        }
                    }
                }
            };

            Node root = new Program { Functions = new List<FunctionDeclaration> { fun1 } };
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

            Program root = new Program();

            var arg1 = new VariableDeclaration()
            {
                VariableType = IntType.Instance,
                Identifier = "Arg1",
            };

            var var1 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var1",
                Value = new IntegerLiteral() { Value = 5 }
            };

            FunctionDeclaration fun1 = new FunctionDeclaration
            {
                Identifier = "Fun1",
                ReturnType = IntType.Instance,
                Parameters = new List<VariableDeclaration> { arg1 },
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression> {
                        var1,
                        new ReturnStatement
                            {
                                Value = new ArithmeticOperation
                                {
                                    OperationType = ArithmeticOperationType.Addition,
                                    LeftValue = new Variable
                                    {
                                        Identifier = "Arg1",
                                        Declaration = arg1
                                    },
                                    RightValue = new Variable
                                    {
                                        Identifier = "Var1",
                                        Declaration = var1
                                    }
                                }
                            }
                        }
                }
            };

            var var2 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var2",
                Value = new FunctionCall
                {
                    Function = "Fun1",
                    Arguments = new List<Expression> { new IntegerLiteral() { Value = 5 } },
                    Declaration = fun1
                }
            };

            FunctionDeclaration fun2 = new FunctionDeclaration
            {
                Identifier = "Fun2",
                ReturnType = IntType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression> {
                        var2,
                        new CompoundAssignment
                        {
                            Lhs = new Variable { Declaration = var2, Identifier = "var2" },
                            Operation = ArithmeticOperationType.Addition,
                            Value = new IntegerLiteral() { Value = 3 }
                        },
                        new ReturnStatement
                            {
                                Value = new Variable { Declaration = var2, Identifier = "var2" }
                            }
                        }
                }
            };
            root.Functions = new List<FunctionDeclaration> { fun1, fun2 };

            return root;
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

            Program root = new Program();

            var arg1 = new VariableDeclaration()
            {
                VariableType = IntType.Instance,
                Identifier = "Arg1",
                Type = UnitType.Instance
            };

            var var1 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var1",
                Value = new IntegerLiteral() { Value = 5, Type = IntType.Instance },
                Type = UnitType.Instance
            };

            FunctionDeclaration fun1 = new FunctionDeclaration
            {
                Identifier = "Fun1",
                ReturnType = IntType.Instance,
                Parameters = new List<VariableDeclaration> { arg1 },
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression> {
                        var1,
                        new ReturnStatement
                            {
                                Value = new ArithmeticOperation
                                {
                                    OperationType = ArithmeticOperationType.Addition,
                                    LeftValue = new Variable
                                    {
                                        Identifier = "Arg1",
                                        Declaration = arg1,
                                        Type = IntType.Instance
                                    },
                                    RightValue = new Variable
                                    {
                                        Identifier = "Var1",
                                        Declaration = var1,
                                        Type = IntType.Instance
                                    },
                                    Type = IntType.Instance
                                },
                                Type = IntType.Instance
                            }
                        },
                    Type = UnitType.Instance
                },
                Type = UnitType.Instance
            };

            var var2 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var2",
                Value = new FunctionCall
                {
                    Function = "Fun1",
                    Arguments = new List<Expression> { new IntegerLiteral() { Value = 5, Type = IntType.Instance } },
                    Declaration = fun1,
                    Type = IntType.Instance
                },
                Type = UnitType.Instance
            };

            FunctionDeclaration fun2 = new FunctionDeclaration
            {
                Identifier = "Fun2",
                ReturnType = IntType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression> {
                        var2,
                        new CompoundAssignment
                        {
                            Lhs = new Variable { Declaration = var2, Identifier = "var2", Type = IntType.Instance },
                            Operation = ArithmeticOperationType.Addition,
                            Value = new IntegerLiteral() { Value = 3, Type = IntType.Instance },
                            Type = IntType.Instance
                        },
                        new ReturnStatement
                            {
                                Value = new Variable { Declaration = var2, Identifier = "var2", Type = IntType.Instance },
                                Type = IntType.Instance
                            }
                        },
                    Type = UnitType.Instance
                },
                Type = UnitType.Instance
            };
            root.Functions = new List<FunctionDeclaration> { fun1, fun2 };

            return root;
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

            Program root = new Program();

            var arg1 = new VariableDeclaration()
            {
                VariableType = BoolType.Instance,
                Identifier = "Arg1",
            };

            var var1 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var1",
                Value = new IntegerLiteral() { Value = 5 }
            };

            FunctionDeclaration fun1 = new FunctionDeclaration
            {
                Identifier = "Fun1",
                ReturnType = IntType.Instance,
                Parameters = new List<VariableDeclaration> { arg1 },
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression> {
                        var1,
                        new ReturnStatement
                            {
                                Value = new ArithmeticOperation
                                {
                                    OperationType = ArithmeticOperationType.Addition,
                                    LeftValue = new Variable
                                    {
                                        Identifier = "Arg1",
                                        Declaration = arg1
                                    },
                                    RightValue = new Variable
                                    {
                                        Identifier = "Var1",
                                        Declaration = var1
                                    }
                                }
                            }
                        }
                }
            };

            var var2 = new VariableDeclaration
            {
                VariableType = IntType.Instance,
                Identifier = "Var2",
                Value = new FunctionCall
                {
                    Function = "Fun1",
                    Arguments = new List<Expression> { new IntegerLiteral() { Value = 5 } },
                    Declaration = fun1
                }
            };

            FunctionDeclaration fun2 = new FunctionDeclaration
            {
                Identifier = "Fun2",
                ReturnType = UnitType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression> {
                        var2,
                        new CompoundAssignment
                        {
                            Lhs = new Variable { Declaration = var2, Identifier = "var2" },
                            Operation = ArithmeticOperationType.Addition,
                            Value = new IntegerLiteral() { Value = 3 }
                        },
                        new ReturnStatement
                            {
                                Value = new Variable { Declaration = var2, Identifier = "var2" }
                            }
                        }
                }
            };
            root.Functions = new List<FunctionDeclaration> { fun1, fun2 };

            return root;
        }

        private Node SimpleAst()
        {
            // aka SimpleAst.json && SimpleAstTyped.json
            Program root = new Program();

            FunctionDeclaration fun1 = new FunctionDeclaration
            {
                Identifier = "Fun1",
                ReturnType = IntType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression>
                        { new ReturnStatement
                            {
                                Value = new IntegerLiteral { Value = 5 }
                            }
                        }
                }
            };

            FunctionDeclaration fun2 = new FunctionDeclaration
            {
                Identifier = "Fun2",
                ReturnType = IntType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock
                {
                    Instructions = new List<Expression>
                    {
                        new ReturnStatement()
                        {
                            Value = new ArithmeticOperation
                            {
                                OperationType = ArithmeticOperationType.Addition,
                                LeftValue = new FunctionCall
                                    {
                                        Function = "Fun1",
                                        Declaration = fun1,
                                        Arguments = new List<Expression>()
                                    },
                                RightValue = new IntegerLiteral { Value = 5 }
                            }
                        }
                    }
                }
            };

            root.Functions = new List<FunctionDeclaration> { fun1, fun2 };

            return root;
        }
    }
}
