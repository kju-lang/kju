namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using KJU.Core.AST;
    using KJU.Core.AST.VariableAccessGraph;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VariableAccessGraphGeneratorTests
    {
        private readonly VariableAccessGraphGenerator generator = new VariableAccessGraphGenerator();

        [TestMethod]
        public void TestEmpty()
        {
            var root = new Program(new List<FunctionDeclaration>());
            var graph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>>();

            var resultExpected = new Dictionary<Node, IReadOnlyCollection<VariableDeclaration>>() {
                { root, new List<VariableDeclaration>() } };

            var resultAccesses = this.generator.BuildVariableAccessesPerAstNode(root, graph);
            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(resultAccesses, resultExpected));

            var resultModifications = this.generator.BuildVariableModificationsPerAstNode(root, graph);
            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(resultModifications, resultExpected));
        }

        [TestMethod]
        public void TestComplex()
        {
            /* Build AST */

            var declarationX = SimpleAstConstruction.CreateVariableDeclaration("x");
            var declarationY = SimpleAstConstruction.CreateVariableDeclaration("y");

            var assignmentX = SimpleAstConstruction.CreateAssignment(
                declarationX, SimpleAstConstruction.CreateVariable(declarationY));
            var incrementY = SimpleAstConstruction.CreateIncrement(declarationY);

            var funInner = SimpleAstConstruction.CreateFunction(
                "funInner",
                new List<Expression>() { assignmentX });

            var funOuter = SimpleAstConstruction.CreateFunction(
                "funOuter",
                new List<Expression>() { declarationX, declarationY, incrementY, funInner });

            var functions = new List<FunctionDeclaration> { funOuter };
            var root = new Program(functions);

            /* Prepare graphs */

            var accessGraph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>>() {
                { funInner, new List<VariableDeclaration>() { declarationX, declarationY } },
                { funOuter, new List<VariableDeclaration>() { declarationY } } };

            var modificationGraph = new Dictionary<FunctionDeclaration, IReadOnlyCollection<VariableDeclaration>>() {
                { funInner, new List<VariableDeclaration>() { declarationX } },
                { funOuter, new List<VariableDeclaration>() { declarationY } } };

            /* Compute accesses per node */

            var accessNothing = new List<Node>() {
                root, funInner, funOuter, declarationX, declarationY,
                declarationX.Value, declarationY.Value, incrementY.Value };

            var accessX = new List<Node>() { assignmentX.Lhs };
            var accessY = new List<Node>() { funOuter.Body, incrementY, incrementY.Lhs, assignmentX.Value };
            var accessBoth = new List<Node>() { assignmentX, funInner.Body };

            var expectedAccesses = new Dictionary<Node, IReadOnlyCollection<VariableDeclaration>>();

            foreach (var node in accessNothing)
                expectedAccesses[node] = new List<VariableDeclaration>() { };

            foreach (var node in accessX)
                expectedAccesses[node] = new List<VariableDeclaration>() { declarationX };

            foreach (var node in accessY)
                expectedAccesses[node] = new List<VariableDeclaration>() { declarationY };

            foreach (var node in accessBoth)
                expectedAccesses[node] = new List<VariableDeclaration>() { declarationX, declarationY };

            /* Compute modifications per node */

            var modifyNothing = new List<Node>() {
                root, funInner, funOuter, declarationX, declarationY, assignmentX.Lhs, assignmentX.Value,
                declarationX.Value, declarationY.Value, incrementY.Value, incrementY.Lhs };

            var modifyX = new List<Node>() { funInner.Body, assignmentX };
            var modifyY = new List<Node>() { funOuter.Body, incrementY };

            var expectedModifications = new Dictionary<Node, IReadOnlyCollection<VariableDeclaration>>();

            foreach (var node in modifyNothing)
                expectedModifications[node] = new List<VariableDeclaration>() { };

            foreach (var node in modifyX)
                expectedModifications[node] = new List<VariableDeclaration>() { declarationX };

            foreach (var node in modifyY)
                expectedModifications[node] = new List<VariableDeclaration>() { declarationY };

            /* Get results from generator and compare */

            var resultAccesses = this.generator.BuildVariableAccessesPerAstNode(root, accessGraph);
            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(resultAccesses, expectedAccesses));

            var resultModifications = this.generator.BuildVariableModificationsPerAstNode(root, modificationGraph);
            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(resultModifications, expectedModifications));
        }
    }
}