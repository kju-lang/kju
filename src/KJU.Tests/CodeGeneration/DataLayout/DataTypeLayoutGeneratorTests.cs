namespace KJU.Tests.CodeGeneration.DataLayout
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.AST.TypeChecker;
    using KJU.Core.AST.Types;
    using KJU.Core.CodeGeneration.DataLayout;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DataTypeLayoutGeneratorTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            var dummyRange = new Core.Lexer.Range(new StringLocation(0), new StringLocation(0));

            var root = new Program(dummyRange, new List<StructDeclaration>(), new List<FunctionDeclaration>());
            this.CheckAnswer(root, new HashSet<DataType>());
        }

        [TestMethod]
        public void TestComplex()
        {
            /*
              def kju() : Unit {
                struct P {
                    x : [Int];
                };

                struct S {
                  x : Int;
                  y : P;
                };

                var s : [S] = new(S, 10);
                var arr : [[Bool]] = new([Bool], 20);
              }
             */

            var dummyRange = new Core.Lexer.Range(new StringLocation(0), new StringLocation(0));

            var pFields = new List<StructField>() {
                new StructField(dummyRange, "x", ArrayType.GetInstance(IntType.Instance)) };

            var pDeclaration = new StructDeclaration(dummyRange, "P", pFields);
            var pType = StructType.GetInstance(pDeclaration);

            var sFields = new List<StructField>() {
                new StructField(dummyRange, "x", IntType.Instance),
                new StructField(dummyRange, "y", pType) };

            var sDeclaration = new StructDeclaration(dummyRange, "S", sFields);
            var sType = StructType.GetInstance(sDeclaration);

            var sAlloc = new ArrayAlloc(
                dummyRange, sType, new IntegerLiteral(dummyRange, 10));
            var sVarDeclaration = new VariableDeclaration(
                dummyRange,
                ArrayType.GetInstance(sType),
                "s",
                sAlloc);

            var arrAlloc = new ArrayAlloc(
                dummyRange, ArrayType.GetInstance(BoolType.Instance), new IntegerLiteral(dummyRange, 20));
            var arrVarDeclaration = new VariableDeclaration(
                dummyRange,
                ArrayType.GetInstance(ArrayType.GetInstance(BoolType.Instance)),
                "arr",
                arrAlloc);

            var kjuInstructions = new List<Expression> {
                pDeclaration,
                sDeclaration,
                sVarDeclaration,
                arrVarDeclaration };

            var kjuDeclaration = new FunctionDeclaration(
                dummyRange,
                "kju",
                ArrayType.GetInstance(UnitType.Instance),
                new List<VariableDeclaration>(),
                new InstructionBlock(dummyRange, kjuInstructions),
                false);

            var root = new Program(dummyRange, new List<StructDeclaration>(), new List<FunctionDeclaration> { kjuDeclaration });

            var expectedTypes = new HashSet<DataType>() {
                pType,
                sType,
                ArrayType.GetInstance(BoolType.Instance),
                ArrayType.GetInstance(IntType.Instance),
                ArrayType.GetInstance(sType),
                ArrayType.GetInstance(ArrayType.GetInstance(BoolType.Instance)) };

            this.CheckAnswer(root, expectedTypes);
        }

        [TestMethod]
        public void TestUnused()
        {
            /*
              def kju() : Unit {
                struct X {};
              }
             */

            var dummyRange = new Core.Lexer.Range(new StringLocation(0), new StringLocation(0));

            var xDeclaration = new StructDeclaration(dummyRange, "X", new List<StructField>());
            var xType = StructType.GetInstance(xDeclaration);

            var kjuInstructions = new List<Expression> { xDeclaration };

            var kjuDeclaration = new FunctionDeclaration(
                dummyRange,
                "kju",
                ArrayType.GetInstance(UnitType.Instance),
                new List<VariableDeclaration>(),
                new InstructionBlock(dummyRange, kjuInstructions),
                false);

            var root = new Program(dummyRange, new List<StructDeclaration>(), new List<FunctionDeclaration> { kjuDeclaration });
            this.CheckAnswer(root, new HashSet<DataType>() { xType });
        }

        [TestMethod]
        public void TestUninitialized()
        {
            /*
              def kju() : Unit {
                var x: [Int];
              }
             */

            var dummyRange = new Core.Lexer.Range(new StringLocation(0), new StringLocation(0));

            var xVarDeclaration = new VariableDeclaration(
                dummyRange, ArrayType.GetInstance(IntType.Instance), "x", null);

            var kjuInstructions = new List<Expression> { xVarDeclaration };

            var kjuDeclaration = new FunctionDeclaration(
                dummyRange,
                "kju",
                ArrayType.GetInstance(UnitType.Instance),
                new List<VariableDeclaration>(),
                new InstructionBlock(dummyRange, kjuInstructions),
                false);

            var root = new Program(dummyRange, new List<StructDeclaration>(), new List<FunctionDeclaration> { kjuDeclaration });
            this.CheckAnswer(root, new HashSet<DataType>() { ArrayType.GetInstance(IntType.Instance) });
        }

        private void CheckAnswer(Node root, HashSet<DataType> expected)
        {
            var diagnosticsMock = new Mock<IDiagnostics>();
            diagnosticsMock.Setup(foo => foo.Add(It.IsAny<Diagnostic[]>())).Throws(new Exception("Diagnostics not empty."));

            var diagnostics = diagnosticsMock.Object;
            var typeChecker = new TypeChecker();

            typeChecker.Run(root, diagnostics);

            var result = new DataTypeLayoutGenerator().CollectTypes(root);
            Assert.IsTrue(result.SetEquals(expected));
        }
    }
}