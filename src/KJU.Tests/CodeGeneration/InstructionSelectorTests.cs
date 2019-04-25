#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
namespace KJU.Tests.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.AST;
    using KJU.Core.CodeGeneration;
    using KJU.Core.CodeGeneration.Templates;
    using KJU.Core.CodeGeneration.Templates.Stack;
    using KJU.Core.Intermediate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InstructionSelectorTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            var template = new RegisterReadTemplate();
            var templates = new List<InstructionTemplate> { template };
            var root = new RegisterRead(new VirtualRegister());
            var tree = new Tree(root) { ControlFlow = new Ret() };
            var selector = new InstructionSelector(templates);
            var ins = selector.Select(tree);
            Assert.AreEqual(2, ins.Count());
        }

        [TestMethod]
        public void StackMemoryTest()
        {
            var template = new ReserveStackMemoryTemplate();
            var templates = new List<InstructionTemplate> { template };
            var root = new ReserveStackMemory(new Function { StackBytes = 16 });
            var tree = new Tree(root) { ControlFlow = new Ret() };
            var selector = new InstructionSelector(templates);
            var ins = selector.Select(tree);
            Assert.AreEqual(2, ins.Count());
            Assert.AreEqual("sub RSP 16", ins.First().ToASM(null));
        }

        [TestMethod]
        public void NullTest()
        {
            InstructionTemplate nullTemplate = new NullTemplate();
            var template = new RegisterReadTemplate();
            var templates = new List<InstructionTemplate> { template, nullTemplate };
            var root = new RegisterRead(new VirtualRegister());
            var tree = new Tree(root) { ControlFlow = new ConditionalJump(new Label(new Tree(new Core.Intermediate.Node())), null) };
            var selector = new InstructionSelector(templates);
            var ins = selector.Select(tree);
            Assert.AreEqual(2, ins.Count());
        }

        [TestMethod]
        public void AddTest()
        {
            var templates = new List<InstructionTemplate> { new AddTemplate(), new RegisterReadTemplate() };
            var v1 = new RegisterRead(new VirtualRegister());
            var v2 = new RegisterRead(new VirtualRegister());
            var v3 = new RegisterRead(new VirtualRegister());
            var node = new ArithmeticBinaryOperation(ArithmeticOperationType.Addition, v1, v2);
            var root = new ArithmeticBinaryOperation(ArithmeticOperationType.Addition, v3, node);
            var tree = new Tree(root) { ControlFlow = new Ret() };
            var selector = new InstructionSelector(templates);
            var ins = selector.Select(tree);
            Assert.AreEqual(6, ins.Count());
        }

        internal class MovRegisterRegisterInstruction : Instruction
        {
            private readonly VirtualRegister to;
            private readonly VirtualRegister from;

            public MovRegisterRegisterInstruction(
                VirtualRegister to,
                VirtualRegister from,
                IReadOnlyCollection<VirtualRegister> uses = null,
                IReadOnlyCollection<VirtualRegister> defines = null,
                IReadOnlyCollection<Tuple<VirtualRegister, VirtualRegister>> copies = null)
                : base(uses, defines, copies)
            {
                this.to = to;
                this.from = from;
            }

            public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                return null;
            }
        }

        internal class NullTemplate : InstructionTemplate
        {
            public NullTemplate()
                : base(null, 0, true)
            {
            }

            public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
            {
                return new NullInstruction();
            }

            private class NullInstruction : Instruction
            {
                public NullInstruction()
                    : base()
                {
                }

                public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
                {
                    return string.Empty;
                }
            }
        }

        internal class AddTemplate : InstructionTemplate
        {
            public AddTemplate()
                : base(new ArithmeticBinaryOperation(ArithmeticOperationType.Addition, null, null), 1)
            {
            }

            public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
            {
                var lhs = fill.GetRegister(0);
                var rhs = fill.GetRegister(1);
                return new AddInstruction(lhs, rhs, result);
            }

            private class AddInstruction : Instruction
            {
                private readonly VirtualRegister lhs;
                private readonly VirtualRegister rhs;
                private readonly VirtualRegister result;

                public AddInstruction(
                    VirtualRegister lhs,
                    VirtualRegister rhs,
                    VirtualRegister result)
                    : base(
                        new List<VirtualRegister> { lhs, rhs },
                        new List<VirtualRegister> { result },
                        new List<Tuple<VirtualRegister, VirtualRegister>>())
                {
                    this.lhs = lhs;
                    this.rhs = rhs;
                    this.result = result;
                }

                public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
                {
                    var lhsHardware = this.lhs.ToHardware(registerAssignment);
                    var rhsHardware = this.rhs.ToHardware(registerAssignment);
                    var resultHardware = this.result.ToHardware(registerAssignment);
                    if (resultHardware.Equals(rhsHardware))
                    {
                        (lhsHardware, rhsHardware) = (rhsHardware, lhsHardware);
                    }

                    var builder = new StringBuilder();
                    if (!resultHardware.Equals(lhsHardware))
                    {
                        builder.AppendLine($"mov {resultHardware} {lhsHardware}");
                    }

                    builder.AppendLine($"add {resultHardware} {rhsHardware}");
                    return builder.ToString();
                }
            }
        }

        internal class RegisterReadTemplate : InstructionTemplate
        {
            public RegisterReadTemplate()
                : base(new RegisterRead(null), 1)
            {
            }

            public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
            {
                var readFrom = fill[0] as VirtualRegister;

                var uses = new List<VirtualRegister> { readFrom };
                var defines = new List<VirtualRegister> { result };
                var copies = new List<Tuple<VirtualRegister, VirtualRegister>>
                {
                    new Tuple<VirtualRegister, VirtualRegister>(
                        readFrom,
                        result)
                };
                return new MovRegisterRegisterInstruction(
                    result,
                    readFrom,
                    uses,
                    defines,
                    copies);
            }
        }
    }
}