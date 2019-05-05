namespace KJU.Tests.Intermediate
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;
    using KJU.Core.Intermediate.Function;
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

            var functionA = new Function(null, "a", new List<KJU.Core.AST.VariableDeclaration>());

            functionA.ReserveStackFrameLocation();
            functionA.ReserveStackFrameLocation();

            var functionB = new Function(functionA, "b", new List<KJU.Core.AST.VariableDeclaration>());

            functionB.ReserveStackFrameLocation();

            var functionC = new Function(functionB, "c", new List<KJU.Core.AST.VariableDeclaration>());

            var variableALocation = functionA.ReserveStackFrameLocation();
            var variableA = new Variable(functionA, variableALocation);

            var variableBLocation = functionB.ReserveStackFrameLocation();
            var variableB = new Variable(functionB, variableBLocation);

            var variableCLocation = new VirtualRegister();
            var variableC = new Variable(functionC, variableCLocation);

            var uniqueNode = new RegisterRead(new VirtualRegister());

            var cReadOperation = (RegisterRead)functionC.GenerateRead(variableC);
            var cReadOperationActualRegister = cReadOperation.Register;
            Assert.AreEqual(variableCLocation, cReadOperationActualRegister);

            var cWriteOperation = (RegisterWrite)functionC.GenerateWrite(variableC, uniqueNode);
            var writeCOperationActualRegister = cWriteOperation.Register;
            var writeCActualValue = cWriteOperation.Value;
            Assert.AreEqual(variableCLocation, writeCOperationActualRegister);
            Assert.AreEqual(uniqueNode, writeCActualValue);

            var bRead = (MemoryRead)functionC.GenerateRead(variableB);
            var computeBRead = (ArithmeticBinaryOperation)bRead.Addr;
/*
            var bReadLeft = (RegisterRead)computeBRead.Lhs;
*/
            var bReadRight = (IntegerImmediateValue)computeBRead.Rhs;
/*
            Assert.AreEqual(cLinkLocation, bReadLeft.Register);
*/
            Assert.AreEqual(variableBLocation.Offset, bReadRight.Value);

            var actualARead = (MemoryRead)functionC.GenerateRead(variableA);
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