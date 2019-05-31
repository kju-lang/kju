namespace KJU.Tests.Intermediate.Function
{
    using System.Collections.Generic;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.Intermediate;
    using KJU.Core.Intermediate.Function;
    using KJU.Core.Intermediate.FunctionGeneration.ReadWrite;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GenerateVariableLocationTest
    {
        [TestMethod]
        public void Test()
        {
            /*
             * fun a():Unit{
             *    var unused1:Int;
             *    var unused2:Int;
             *    var a:Int;
             *    fun b():Unit{
             *        var unused3:Int;
             *        var b:Int;
             *            fun c():Unit{
             *                var c:Int;
             *                var uniqueNode:Int;
             *                 c=uniqueNode;
             *                 b;
             *                 c;
             *            }
             *    }
             * }
             *
             */
            var functionInfoA = new Function(null, "a", new List<VariableDeclaration>(), false, false);
            functionInfoA.ReserveClosureLocation("dummy", IntType.Instance);
            functionInfoA.ReserveClosureLocation("dummy", IntType.Instance);
            functionInfoA.ReserveClosureLocation("dummy", IntType.Instance);
            functionInfoA.ReserveClosureLocation("dummy", IntType.Instance);
            var functionInfoB = new Function(functionInfoA, "b", new List<VariableDeclaration>(), false, false);
            functionInfoB.ReserveClosureLocation("dummy", IntType.Instance);
            functionInfoB.ReserveClosureLocation("dummy", IntType.Instance);
            functionInfoB.ReserveClosureLocation("dummy", IntType.Instance);
            var functionInfoC = new Function(functionInfoB, "c", new List<VariableDeclaration>(), false, false);
            functionInfoC.ReserveClosureLocation("dummy", IntType.Instance);
            functionInfoC.ReserveClosureLocation("dummy", IntType.Instance);

            var variableALocation = functionInfoA.ReserveClosureLocation("a", IntType.Instance);
            var variableA = variableALocation;

            var variableBLocation = functionInfoB.ReserveClosureLocation("b", IntType.Instance);
            var variableB = variableBLocation;

            var variableCLocation = new VirtualRegister();
            var variableC = variableCLocation;

            var uniqueNode = new RegisterRead(new VirtualRegister());

            var readWriteGenerator = new ReadWriteGenerator();

            var cReadOperation = (RegisterRead)readWriteGenerator.GenerateRead(functionInfoC, variableC);
            var cReadOperationActualRegister = cReadOperation.Register;
            Assert.AreEqual(variableCLocation, cReadOperationActualRegister);

            var cWriteOperation = (RegisterWrite)readWriteGenerator.GenerateWrite(functionInfoC, variableC, uniqueNode);
            var writeCOperationActualRegister = cWriteOperation.Register;
            var writeCActualValue = cWriteOperation.Value;
            Assert.AreEqual(variableCLocation, writeCOperationActualRegister);
            Assert.AreEqual(uniqueNode, writeCActualValue);

            var bRead = (MemoryRead)readWriteGenerator.GenerateRead(functionInfoC, variableB);
            var computeBRead = (ArithmeticBinaryOperation)bRead.Addr;
/*
            var bReadLeft = (RegisterRead)computeBRead.Lhs;
*/
            var bReadRight = (IntegerImmediateValue)computeBRead.Rhs;
/*
            Assert.AreEqual(cLinkLocation, bReadLeft.Register);
*/
            Assert.AreEqual(variableBLocation.Offset, bReadRight.Value);

            var actualARead = (MemoryRead)readWriteGenerator.GenerateRead(functionInfoC, variableA);
            var actualAAddress = (ArithmeticBinaryOperation)actualARead.Addr;
/*
            var actualAAddressLeft = (MemoryRead)actualAAddress.Lhs;
*/
            var actualAAddressRight = (IntegerImmediateValue)actualAAddress.Rhs;
/*
            var actualAStackAddress = (ArithmeticBinaryOperation)actualAAddressLeft.Addr; // bLink
*/
/*
            var actualAStackAddressLeft = (RegisterRead)actualAStackAddress.Lhs;
            var actualAStackAddressRight = (IntegerImmediateValue)actualAStackAddress.Rhs;
*/
            Assert.AreEqual(variableALocation.Offset, actualAAddressRight.Value);
/*
            Assert.AreEqual(functionBLinkLocation.Offset, actualAStackAddressRight.Value);
            Assert.AreEqual(cLinkLocation, actualAStackAddressLeft.Register);
*/
        }
    }
}
