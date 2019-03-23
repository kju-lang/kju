﻿namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NameResolverTests
    {
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
                var functionCall = new FunctionCall { Function = "b", Arguments = new List<Expression>() };
                var expressions = new List<Expression> { functionCall };
                var fun = new FunctionDeclaration
                {
                    Identifier = id,
                    Type = UnitType.Instance,
                    Body = new InstructionBlock
                    {
                        Instructions = expressions
                    },
                    Parameters = new List<VariableDeclaration>()
                };
                calls.Add(functionCall);
                functions.Add(fun);
            }

            var root = new Program { Functions = functions };
            var resolver = new NameResolver();
            resolver.LinkNames(root, null);
            var bDeclaration = functions[1];
            var expected = new List<FunctionDeclaration> { bDeclaration, bDeclaration, bDeclaration };
            var actual = calls.Select(x => x.Declaration).ToList();
            CollectionAssert.AreEqual(expected, actual);
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
            var h = new FunctionDeclaration
            {
                Identifier = "h",
                Type = UnitType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression>() }
            };
            var x = new VariableDeclaration { Identifier = "x", Type = IntType.Instance };
            var v = new Variable { Identifier = "x" };
            var h2 = new VariableDeclaration { Identifier = "h", Type = IntType.Instance };
            var v3 = new Variable { Identifier = "h" };
            var f = new FunctionDeclaration
            {
                Identifier = "f",
                Type = UnitType.Instance,
                Parameters = new List<VariableDeclaration> { x },
                Body = new InstructionBlock { Instructions = new List<Expression> { v, h2, v3 } }
            };

            var x2 = new VariableDeclaration { Identifier = "x", Type = IntType.Instance };
            var v2 = new Variable { Identifier = "x" };
            var fc = new FunctionCall { Function = "f", Arguments = new List<Expression> { v2 } };

            var g = new FunctionDeclaration
            {
                Identifier = "g",
                Type = UnitType.Instance,
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression> { x2, fc } }
            };

            var root = new Program { Functions = new List<FunctionDeclaration> { h, f, g } };

            var resolver = new NameResolver();
            resolver.LinkNames(root, null);
            var expected = new List<Node> { x, h2, x2, f };
            var actual = new List<Node> { v.Declaration, v3.Declaration, v2.Declaration, fc.Declaration };
            CollectionAssert.AreEqual(expected, actual);
        }

        /*
         * fun f()
         * {
         *   var x;
         *   var x;
         * }
         */
        [TestMethod]
        public void TestVariableRedeclaration()
        {
            var x = new VariableDeclaration { Identifier = "x" };
            var x2 = new VariableDeclaration { Identifier = "x" };
            var f = new FunctionDeclaration
            {
                Identifier = "f",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression> { x, x2 } }
            };

            var diagnostics = new Diagnostics();

            var resolver = new NameResolver();
            var root = new Program { Functions = new List<FunctionDeclaration> { f } };

            Assert.ThrowsException<NameResolverException>(() => resolver.LinkNames(root, diagnostics));
            Assert.AreEqual(diagnostics.Diag.Count, 1);
            Assert.AreEqual(diagnostics.Diag[0].Message, "Multiple declarations of name x");
        }

        /*
         * fun f()
         * {
         * }
         * fun f()
         * {
         * }
         */
        [TestMethod]
        public void TestFunctionRedeclaration()
        {
            var f = new FunctionDeclaration
            {
                Identifier = "f",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression>() }
            };

            var f2 = new FunctionDeclaration
            {
                Identifier = "f",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression>() }
            };

            var diagnostics = new Diagnostics();

            var resolver = new NameResolver();
            var root = new Program { Functions = new List<FunctionDeclaration> { f, f2 } };

            Assert.ThrowsException<NameResolverException>(() => resolver.LinkNames(root, diagnostics));
            Assert.AreEqual(diagnostics.Diag[0].Message, "Multiple declarations of name f");
        }

        /*
         * fun f()
         * {
         *   fun g()
         *   {
         *   };
         *   fun g()
         *   {
         *   };
         * }
         */
        [TestMethod]
        public void TestInnerFunctionRedeclaration()
        {
            var g = new FunctionDeclaration
            {
                Identifier = "g",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression>() }
            };

            var g2 = new FunctionDeclaration
            {
                Identifier = "g",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression>() }
            };

            var f = new FunctionDeclaration
            {
                Identifier = "f",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression> { g, g2 } }
            };

            var diagnostics = new Diagnostics();

            var resolver = new NameResolver();
            var root = new Program { Functions = new List<FunctionDeclaration> { f } };

            Assert.ThrowsException<NameResolverException>(() => resolver.LinkNames(root, diagnostics));
            Assert.AreEqual(diagnostics.Diag.Count, 1);
            Assert.AreEqual(diagnostics.Diag[0].Message, "Multiple declarations of name g");
        }

        /*
         * fun f()
         * {
         *   x;
         *   var x;
         *   g();
         * }
         */
        [TestMethod]
        public void TestNoDeclaration()
        {
            var x = new VariableDeclaration { Identifier = "x" };

            var v = new Variable { Identifier = "x" };

            var fc = new FunctionCall { Function = "g", Arguments = new List<Expression>() };

            var f = new FunctionDeclaration
            {
                Identifier = "f",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression> { v, x, fc } }
            };

            var diagnostics = new Diagnostics();

            var resolver = new NameResolver();
            var root = new Program { Functions = new List<FunctionDeclaration> { f } };

            Assert.ThrowsException<NameResolverException>(() => resolver.LinkNames(root, diagnostics));
            Assert.AreEqual(diagnostics.Diag.Count, 2);
            Assert.AreEqual(diagnostics.Diag[0].Message, "No identifier of name x");
            Assert.AreEqual(diagnostics.Diag[1].Message, "No identifier of name g");
        }

        /*
         * fun f()
         * {
         *   f;
         *   var f;
         *   f();
         * }
         */
        [TestMethod]
        public void TestWrongDeclaration()
        {
            var x = new VariableDeclaration { Identifier = "f" };

            var v = new Variable { Identifier = "f" };

            var fc = new FunctionCall { Function = "f", Arguments = new List<Expression>() };

            var f = new FunctionDeclaration
            {
                Identifier = "f",
                Parameters = new List<VariableDeclaration>(),
                Body = new InstructionBlock { Instructions = new List<Expression> { v, x, fc } }
            };

            var diagnostics = new Diagnostics();

            var resolver = new NameResolver();
            var root = new Program { Functions = new List<FunctionDeclaration> { f } };

            Assert.ThrowsException<NameResolverException>(() => resolver.LinkNames(root, diagnostics));
            Assert.AreEqual(diagnostics.Diag.Count, 2);
            Assert.AreEqual(diagnostics.Diag[0].Message, "f is not a variable");
            Assert.AreEqual(diagnostics.Diag[1].Message, "f is not a function");
        }

        private class Diagnostics : IDiagnostics
        {
            public Diagnostics()
            {
                this.Diag = new List<Diagnostic>();
            }

            public List<Diagnostic> Diag { get; }

            public void Add(params Diagnostic[] diagnostics)
            {
                this.Diag.AddRange(diagnostics);
            }

            public void Report()
            {
            }
        }
    }
}