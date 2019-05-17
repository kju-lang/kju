namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.CallGraph;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CallGraphTests
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
        public void TestCallGraph()
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
                calls.Add(functionCall);
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
                functions.Add(fun);
            }

            foreach (var call in calls)
            {
                call.Declaration = functions[1];
            }

            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            CallGraphGenerator gen = new CallGraphGenerator();
            var dict = gen.BuildCallGraph(root);

            Assert.AreEqual(1, dict[functions[0]].Count);
            Assert.AreEqual(1, dict[functions[1]].Count);
            Assert.AreEqual(1, dict[functions[2]].Count);

            Assert.IsTrue(dict[functions[0]].Contains(functions[1]));
            Assert.IsTrue(dict[functions[1]].Contains(functions[1]));
            Assert.IsTrue(dict[functions[2]].Contains(functions[1]));
        }

        /*
         * fun a()
         * {
         *   b();
         *   fun c()
         *   {
         *      c();
         *   }
         * }
         * fun b()
         * {
         *   a();
         * }
         */
        [TestMethod]
        public void TestInnerFunction()
        {
            var functions = new List<FunctionDeclaration>();
            var aCall = new FunctionCall(
                new Range(new StringLocation(0), new StringLocation(1)),
                "a",
                new List<Expression>());
            var instructionBlock = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { aCall });
            var b = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "b",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                instructionBlock,
                false);
            functions.Add(b);

            var cCall = new FunctionCall(
                new Range(new StringLocation(0), new StringLocation(1)),
                "c",
                new List<Expression>());

            var block = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { cCall });
            var c = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "c",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                block,
                false);

            var bCall = new FunctionCall(
                new Range(new StringLocation(0), new StringLocation(1)),
                "b",
                new List<Expression>());

            var body = new InstructionBlock(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<Expression> { bCall, c });
            var a = new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                "a",
                UnitType.Instance,
                new List<VariableDeclaration>(),
                body,
                false);
            functions.Add(a);

            var root = new Program(
                new Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                new List<FunctionDeclaration> { a, b });

            aCall.Declaration = a;
            bCall.Declaration = b;
            cCall.Declaration = c;

            CallGraphGenerator gen = new CallGraphGenerator();
            var dict = gen.BuildCallGraph(root);

            Assert.AreEqual(1, dict[b].Count);
            Assert.AreEqual(1, dict[a].Count);
            Assert.AreEqual(1, dict[c].Count);

            Assert.IsTrue(dict[b].Contains(a));
            Assert.IsTrue(dict[c].Contains(c));
            Assert.IsTrue(dict[a].Contains(b));
        }
    }
}