namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Diagnostics;
    using KJU.Core.AST;
    using KJU.Core.AST.CallGraph;
    using KJU.Core.AST.VariableAccessGraph;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;

    [TestClass]
    public class VariableAccessGraphGeneratorTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            var root = new Program(
                new Core.Lexer.Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                new List<FunctionDeclaration>());
            var graph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>>();

            var resultExpected = new Dictionary<Node, IReadOnlyCollection<VariableDeclaration>>()
            {
                { root, new List<VariableDeclaration>() }
            };

            var callGraphGenerator = new CallGraphGenerator();
            var nodeInfoExtractors = new Dictionary<VariableInfo, INodeInfoExtractor>
            {
                [VariableInfo.Access] = new AccessInfoExtractor(),
                [VariableInfo.Modifications] = new ModifyInfoExtractor(),
            };
            var generator = new VariableAccessGraphGenerator(callGraphGenerator, nodeInfoExtractors);
            var resultActual = generator.GetVariableInfoPerAstNode(root);

            MappingEquivalence.AssertAreEquivalentCollection(resultExpected, resultActual.Accesses);
            MappingEquivalence.AssertAreEquivalentCollection(resultExpected, resultActual.Modifies);
        }

        [TestMethod]
        public void TestComplex()
        {
            /* Build AST */

            var declarationX = AstConstructionUtils.CreateVariableDeclaration("x");
            var declarationY = AstConstructionUtils.CreateVariableDeclaration("y");

            var assignmentX = AstConstructionUtils.CreateAssignment(
                declarationX,
                AstConstructionUtils.CreateVariable(declarationY));
            var incrementY = AstConstructionUtils.CreateIncrement(declarationY);

            var funInner = AstConstructionUtils.CreateFunction(
                "funInner",
                new List<Expression> { assignmentX });

            var funOuter = AstConstructionUtils.CreateFunction(
                "funOuter",
                new List<Expression> { declarationX, declarationY, incrementY, funInner });

            var functions = new List<FunctionDeclaration> { funOuter };
            var root = new Program(
                new Core.Lexer.Range(new StringLocation(0), new StringLocation(1)),
                new List<StructDeclaration>(),
                functions);

            /* Prepare graphs */

            var accessGraph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>>()
            {
                { funInner, new List<VariableDeclaration> { declarationX, declarationY } },
                { funOuter, new List<VariableDeclaration> { declarationY } }
            };

            var modificationGraph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>>()
            {
                { funInner, new List<VariableDeclaration> { declarationX } },
                { funOuter, new List<VariableDeclaration> { declarationY } }
            };

            /* Compute accesses per node */

            var accessNothing = new List<Node>
            {
                root, funInner, funOuter, declarationX, declarationY,
                declarationX.Value, declarationY.Value, incrementY.Value
            };

            var accessX = new List<Node> { assignmentX.Lhs };
            var accessY = new List<Node> { funOuter.Body, incrementY, incrementY.Lhs, assignmentX.Value };
            var accessBoth = new List<Node> { assignmentX, funInner.Body };

            var expectedAccesses = new Dictionary<Node, IReadOnlyCollection<VariableDeclaration>>();

            foreach (var node in accessNothing)
            {
                expectedAccesses[node] = new List<VariableDeclaration>();
            }

            foreach (var node in accessX)
            {
                expectedAccesses[node] = new List<VariableDeclaration> { declarationX };
            }

            foreach (var node in accessY)
            {
                expectedAccesses[node] = new List<VariableDeclaration> { declarationY };
            }

            foreach (var node in accessBoth)
            {
                expectedAccesses[node] = new List<VariableDeclaration> { declarationX, declarationY };
            }

            /* Compute modifications per node */

            var modifyNothing = new List<Node>
            {
                root, funInner, funOuter, declarationX, declarationY, assignmentX.Lhs, assignmentX.Value,
                declarationX.Value, declarationY.Value, incrementY.Value, incrementY.Lhs
            };

            var modifyX = new List<Node> { funInner.Body, assignmentX };
            var modifyY = new List<Node> { funOuter.Body, incrementY };

            var expectedModifications = new Dictionary<Node, IReadOnlyCollection<VariableDeclaration>>();

            foreach (var node in modifyNothing)
            {
                expectedModifications[node] = new List<VariableDeclaration>();
            }

            foreach (var node in modifyX)
            {
                expectedModifications[node] = new List<VariableDeclaration> { declarationX };
            }

            foreach (var node in modifyY)
            {
                expectedModifications[node] = new List<VariableDeclaration> { declarationY };
            }

            /* Get results from generator and compare */

            var callGraphGenerator = new CallGraphGenerator();
            var nodeInfoExtractors = new Dictionary<VariableInfo, INodeInfoExtractor>
            {
                [VariableInfo.Access] = new AccessInfoExtractor(),
                [VariableInfo.Modifications] = new ModifyInfoExtractor(),
            };
            var generator = new VariableAccessGraphGenerator(callGraphGenerator, nodeInfoExtractors);
            var resultActual = generator.GetVariableInfoPerAstNode(root);
            MappingEquivalence.AssertAreEquivalentCollection(expectedAccesses, resultActual.Accesses);
            MappingEquivalence.AssertAreEquivalentCollection(expectedModifications, resultActual.Modifies);
        }

        [TestMethod]
        public void DirectVariableAccessTest()
        {
            string program = @"
                fun kju (param : Int) : Unit {
                    var x : Int;
                    fun f (par1 : Bool) : Int {
                        return x;
                    };
                }";
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            var ast = KjuCompilerUtils.MakeAstWithLinkedNames(program, diagnostics);
            var identifiersMap = this.CreateIdDictionary(ast);
            var functions = identifiersMap
                .Where(p => p.Value is FunctionDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as FunctionDeclaration);
            var variables = identifiersMap
                .Where(p => p.Value is VariableDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as VariableDeclaration);
            var mockCallGraph = new Mock<ICallGraphGenerator>();
            var callGraphDict = new Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>>()
            {
                [functions["kju"]] = new HashSet<FunctionDeclaration>(),
                [functions["f"]] = new HashSet<FunctionDeclaration>(),
            };
            mockCallGraph.Setup(foo => foo.BuildCallGraph(It.IsAny<Node>())).Returns(callGraphDict);
            var nodeInfoExtractors = new Dictionary<VariableInfo, INodeInfoExtractor>
            {
                [VariableInfo.Access] = new AccessInfoExtractor(),
                [VariableInfo.Modifications] = new ModifyInfoExtractor(),
            };

            INodeInfoExtractor infoExtractor = new AccessInfoExtractor();
            var graph = VariableAccessGraphGenerator.TransitiveCallClosure(mockCallGraph.Object, ast, infoExtractor);

            Assert.IsTrue(graph[functions["f"]].Contains(variables["x"]));
            Assert.IsFalse(graph[functions["kju"]].Contains(variables["par1"]));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        [TestMethod]
        public void IndirectVariableAccessTest()
        {
            string program = @"
                fun kju (param : Int) : Unit {
                    var x : Int;
                    fun f (par1 : Int, par2 : Int) : Int {
                        x;
                        return par1;
                    };
                    fun g () : Unit {
                        f(1, 2);
                    };
                    g();
                }";
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            var ast = KjuCompilerUtils.MakeAstWithLinkedNames(program, diagnostics);
            var identifiersMap = this.CreateIdDictionary(ast);
            var functions = identifiersMap
                .Where(p => p.Value is FunctionDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as FunctionDeclaration);
            var variables = identifiersMap
                .Where(p => p.Value is VariableDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as VariableDeclaration);
            var mockCallGraph = new Mock<ICallGraphGenerator>();
            var callGraphDict = new Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>>
            {
                [functions["kju"]] = new HashSet<FunctionDeclaration>() { functions["f"] },
                [functions["f"]] = new HashSet<FunctionDeclaration>(),
                [functions["g"]] = new HashSet<FunctionDeclaration>() { functions["f"] },
            };
            mockCallGraph.Setup(foo => foo.BuildCallGraph(It.IsAny<Node>())).Returns(callGraphDict);

            var nodeInfoExtractors = new Dictionary<VariableInfo, INodeInfoExtractor>
            {
                [VariableInfo.Access] = new AccessInfoExtractor(),
                [VariableInfo.Modifications] = new ModifyInfoExtractor(),
            };

            VariableAccessGraphGenerator tempQualifier =
                new VariableAccessGraphGenerator(mockCallGraph.Object, nodeInfoExtractors);
            INodeInfoExtractor infoExtractor = new AccessInfoExtractor();
            var graph = VariableAccessGraphGenerator.TransitiveCallClosure(mockCallGraph.Object, ast, infoExtractor);

            Assert.IsTrue(graph[functions["kju"]].Contains(variables["x"]));
            Assert.IsTrue(graph[functions["kju"]].Contains(variables["par1"]));
            Assert.IsTrue(graph[functions["g"]].Contains(variables["x"]));
            Assert.IsTrue(graph[functions["g"]].Contains(variables["par1"]));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        [TestMethod]
        public void DirectVariableModificationTest()
        {
            string program = @"
                fun kju (param : Int) : Unit {
                    var x : Int;
                    var y : Int;
                    fun f (par1 : Bool) : Unit {
                        x = 5;
                        y += 10;
                    };
                }";
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            var ast = KjuCompilerUtils.MakeAstWithLinkedNames(program, diagnostics);
            var identifiersMap = this.CreateIdDictionary(ast);
            var functions = identifiersMap
                .Where(p => p.Value is FunctionDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as FunctionDeclaration);
            var variables = identifiersMap
                .Where(p => p.Value is VariableDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as VariableDeclaration);
            var mockCallGraph = new Mock<ICallGraphGenerator>();
            var callGraphDict = new Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>>()
            {
                [functions["kju"]] = new HashSet<FunctionDeclaration>(),
                [functions["f"]] = new HashSet<FunctionDeclaration>(),
            };
            mockCallGraph.Setup(foo => foo.BuildCallGraph(It.IsAny<Node>())).Returns(callGraphDict);

            var nodeInfoExtractors = new Dictionary<VariableInfo, INodeInfoExtractor>
            {
                [VariableInfo.Access] = new AccessInfoExtractor(),
                [VariableInfo.Modifications] = new ModifyInfoExtractor(),
            };

            VariableAccessGraphGenerator tempQualifier =
                new VariableAccessGraphGenerator(mockCallGraph.Object, nodeInfoExtractors);
            INodeInfoExtractor infoExtractor = new AccessInfoExtractor();
            var graph = VariableAccessGraphGenerator.TransitiveCallClosure(mockCallGraph.Object, ast, infoExtractor);

            Assert.IsTrue(graph[functions["f"]].Contains(variables["x"]));
            Assert.IsTrue(graph[functions["f"]].Contains(variables["y"]));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        [TestMethod]
        public void IndirectVariableModificationTest()
        {
            string program = @"
                fun kju (param : Int) : Unit {
                    var x : Int;
                    var y : Int;
                    fun f (par1 : Bool) : Unit {
                        x = 5;
                        y += 10;
                    };
                    fun g () : Unit {
                        f();
                    };
                    fun h () : Unit {
                        g();
                    };
                }";
            var diagnosticsMock = new Mock<IDiagnostics>();
            var diagnostics = diagnosticsMock.Object;
            var ast = KjuCompilerUtils.MakeAstWithLinkedNames(program, diagnostics);
            var identifiersMap = this.CreateIdDictionary(ast);
            var functions = identifiersMap
                .Where(p => p.Value is FunctionDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as FunctionDeclaration);
            var variables = identifiersMap
                .Where(p => p.Value is VariableDeclaration)
                .ToDictionary(p => p.Key, p => p.Value as VariableDeclaration);
            var mockCallGraph = new Mock<ICallGraphGenerator>();
            var callGraphDict = new Dictionary<FunctionDeclaration, IReadOnlyCollection<FunctionDeclaration>>()
            {
                [functions["kju"]] = new HashSet<FunctionDeclaration>(),
                [functions["f"]] = new HashSet<FunctionDeclaration>(),
                [functions["g"]] = new HashSet<FunctionDeclaration>() { functions["f"] },
                [functions["h"]] = new HashSet<FunctionDeclaration>() { functions["g"] },
            };
            mockCallGraph.Setup(foo => foo.BuildCallGraph(It.IsAny<Node>())).Returns(callGraphDict);

            var nodeInfoExtractors = new Dictionary<VariableInfo, INodeInfoExtractor>
            {
                [VariableInfo.Access] = new AccessInfoExtractor(),
                [VariableInfo.Modifications] = new ModifyInfoExtractor(),
            };

            VariableAccessGraphGenerator tempQualifier =
                new VariableAccessGraphGenerator(mockCallGraph.Object, nodeInfoExtractors);
            INodeInfoExtractor infoExtractor = new AccessInfoExtractor();
            var graph = VariableAccessGraphGenerator.TransitiveCallClosure(mockCallGraph.Object, ast, infoExtractor);

            Assert.IsTrue(graph[functions["g"]].Contains(variables["x"]));
            Assert.IsTrue(graph[functions["g"]].Contains(variables["y"]));
            Assert.IsTrue(graph[functions["h"]].Contains(variables["x"]));
            Assert.IsTrue(graph[functions["h"]].Contains(variables["y"]));
            MockDiagnostics.Verify(diagnosticsMock);
        }

        private Dictionary<string, Node> CreateIdDictionary(Node root)
        {
            var result = root
                .Children()
                .SelectMany(this.CreateIdDictionary)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            switch (root)
            {
                case FunctionDeclaration functionDeclaration:
                    result.Add(functionDeclaration.Identifier, functionDeclaration);
                    break;
                case VariableDeclaration variableDeclaration:
                    result.Add(variableDeclaration.Identifier, variableDeclaration);
                    break;
            }

            return result;
        }
    }
}