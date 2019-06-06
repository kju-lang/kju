namespace KJU.Tests.AST
{
    using System.Collections.Generic;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.TypeChecker;
    using KJU.Core.AST.Types;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Util;

    [TestClass]
    public class SolutionNormalizerTests
    {
        [TestMethod]
        public void TestSimple()
        {
            var var0 = new TypeVariable();
            var var1 = new TypeVariable();

            var input = new Dictionary<TypeVariable, IHerbrandObject>() {
                { var0, IntType.Instance }, { var1, BoolType.Instance } };

            var output = new Dictionary<TypeVariable, IHerbrandObject>() {
                { var0, IntType.Instance }, { var1, BoolType.Instance } };

            CheckOutput(input, output);
        }

        [TestMethod]
        public void TestUninstantiated()
        {
            var var0 = new TypeVariable();
            var var1 = new TypeVariable();

            var rhs = new FunType(
                new List<DataType>() {
                    BoolType.Instance,
                    new ArrayType(new ArrayType(var1)),
                    BoolType.Instance },
                IntType.Instance);

            var input = new Dictionary<TypeVariable, IHerbrandObject>() { { var0, rhs } };
            CheckThrows(input);
        }

        [TestMethod]
        public void TestArrayRecursion()
        {
            var var0 = new TypeVariable();
            var var1 = new TypeVariable();
            var var2 = new TypeVariable();

            var rhs = new FunType(
                new List<DataType>() {
                    BoolType.Instance,
                    new ArrayType(new ArrayType(var1)),
                    BoolType.Instance },
                IntType.Instance);

            var input = new Dictionary<TypeVariable, IHerbrandObject>() {
                { var0, new FunType(new List<DataType>(), var1) },
                { var1, new ArrayType(var2) },
                { var2, new FunType(new List<DataType>() { var0 }, UnitType.Instance) } };

            CheckThrows(input);
        }

        [TestMethod]
        public void TestNested()
        {
            var var0 = new TypeVariable();
            var var1 = new TypeVariable();
            var var2 = new TypeVariable();

            var input = new Dictionary<TypeVariable, IHerbrandObject>() {
                { var0, new FunType(new List<DataType>(), new FunType(new List<DataType>(), var1)) },
                { var1, new ArrayType(new ArrayType(new ArrayType(var2))) },
                { var2, IntType.Instance } };

            var output2 = IntType.Instance;
            var output1 = new ArrayType(new ArrayType(new ArrayType(output2)));
            var output0 = new FunType(new List<DataType>(), new FunType(new List<DataType>(), output1));

            var output = new Dictionary<TypeVariable, IHerbrandObject>() {
                { var0, output0 }, { var1, output1 }, { var2, output2 } };

            CheckOutput(input, output);
        }

        [TestMethod]
        public void TestFunRecursion()
        {
            var var = new TypeVariable();

            var input = new Dictionary<TypeVariable, IHerbrandObject>() {
                { var, new FunType(new List<DataType>() { IntType.Instance }, var) } };

            var expectedType = new FunType(new List<DataType>() { IntType.Instance }, null);
            expectedType.ResultType = expectedType;

            var output = new Dictionary<TypeVariable, IHerbrandObject>() { { var, expectedType } };

            CheckOutput(input, output);
        }

        private static IDictionary<TypeVariable, IHerbrandObject> GetOutput(IDictionary<TypeVariable, IHerbrandObject> input)
        {
            var chosenAlternative = new Mock<IDictionary<Clause, int>>();
            var solution = new Solution() { TypeVariableMapping = input, ChosenAlternative = chosenAlternative.Object };

            var normalizedSolution = SolutionNormalizer.Normalize(solution);

            chosenAlternative.VerifyNoOtherCalls();
            Assert.ReferenceEquals(normalizedSolution.ChosenAlternative, chosenAlternative);

            return normalizedSolution.TypeVariableMapping;
        }

        private static void CheckOutput(IDictionary<TypeVariable, IHerbrandObject> input, IDictionary<TypeVariable, IHerbrandObject> expectedOutput)
        {
            var output = GetOutput(input);
            Assert.IsTrue(MappingEquivalence.AreEquivalent(
                (IReadOnlyDictionary<TypeVariable, IHerbrandObject>)output,
                (IReadOnlyDictionary<TypeVariable, IHerbrandObject>)expectedOutput));
        }

        private static void CheckThrows(IDictionary<TypeVariable, IHerbrandObject> input)
        {
            Assert.ThrowsException<SolutionNormalizerException>(() => GetOutput(input));
        }
    }
}