namespace KJU.Tests.Intermediate
{
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

            var functionA = new Function();

            functionA.ReserveStackFrameLocation();
            functionA.ReserveStackFrameLocation();

            var functionB = new Function { Parent = functionA };
            var functionBLinkLocation = functionB.ReserveStackFrameLocation();
            functionB.Link = new Variable { Location = functionBLinkLocation, Owner = functionB };

            functionB.ReserveStackFrameLocation();

            var functionC = new Function { Parent = functionB };
            var cLinkLocation = new VirtualRegister();
            functionC.Link = new Variable { Location = cLinkLocation, Owner = functionC };

            var variableALocation = functionA.ReserveStackFrameLocation();
            var variableA = new Variable { Owner = functionA, Location = variableALocation };

            var variableBLocation = functionB.ReserveStackFrameLocation();
            var variableB = new Variable { Owner = functionB, Location = variableBLocation };

            var variableCLocation = new VirtualRegister();
            var variableC = new Variable { Owner = functionC, Location = variableCLocation };

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
            var bReadLeft = (RegisterRead)computeBRead.Lhs;
            var bReadRight = (IntegerImmediateValue)computeBRead.Rhs;
            Assert.AreEqual(cLinkLocation, bReadLeft.Register);
            Assert.AreEqual(variableBLocation.Offset, bReadRight.Value);

            var actualARead = (MemoryRead)functionC.GenerateRead(variableA);
            var actualAAddress = (ArithmeticBinaryOperation)actualARead.Addr;
            var actualAAddressLeft = (MemoryRead)actualAAddress.Lhs;
            var actualAAddressRight = (IntegerImmediateValue)actualAAddress.Rhs;
            var actualAStackAddress = (ArithmeticBinaryOperation)actualAAddressLeft.Addr; // bLink
            var actualAStackAddressLeft = (RegisterRead)actualAStackAddress.Lhs;
            var actualAStackAddressRight = (IntegerImmediateValue)actualAStackAddress.Rhs;
            Assert.AreEqual(variableALocation.Offset, actualAAddressRight.Value);
            Assert.AreEqual(functionBLinkLocation.Offset, actualAStackAddressRight.Value);
            Assert.AreEqual(cLinkLocation, actualAStackAddressLeft.Register);
        }
    }
}