namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.CallGraph;
    using KJU.Core.Diagnostics;
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
                var functionCall = new FunctionCall("b", new List<Expression>());
                calls.Add(functionCall);
                var body = new InstructionBlock(new List<Expression> { functionCall });
                var fun = new FunctionDeclaration(
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

            var root = new Program(functions);

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
            var aCall = new FunctionCall("a", new List<Expression>());
            var b = new FunctionDeclaration(
                    "b",
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    new InstructionBlock(new List<Expression> { aCall }),
                    false);
            functions.Add(b);

            var cCall = new FunctionCall("c", new List<Expression>());

            var c = new FunctionDeclaration(
                    "c",
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    new InstructionBlock(new List<Expression> { cCall }),
                    false);

            var bCall = new FunctionCall("b", new List<Expression>());

            var a = new FunctionDeclaration(
                    "a",
                    UnitType.Instance,
                    new List<VariableDeclaration>(),
                    new InstructionBlock(new List<Expression> { bCall, c }),
                    false);
            functions.Add(a);

            var root = new Program(new List<FunctionDeclaration> { a, b });

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