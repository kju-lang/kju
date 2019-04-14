#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
namespace KJU.Tests.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.AST;
    using KJU.Core.CodeGeneration;
    using KJU.Core.CodeGeneration.Templates;
    using KJU.Core.Intermediate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InstructionSelectorTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            InstructionTemplate template = new RegisterReadTemplate();
            List<InstructionTemplate> templates = new List<InstructionTemplate> { template };
            var root = new RegisterRead(new VirtualRegister());
            Tree tree = new Tree(root);
            tree.ControlFlow = new Ret();
            InstructionSelector selector = new InstructionSelector(templates);
            var ins = selector.Select(tree) as List<Instruction>;
            Assert.AreEqual(2, ins.Count);
        }

        [TestMethod]
        public void AddTest()
        {
            List<InstructionTemplate> templates = new List<InstructionTemplate> { new AddTemplate(), new RegisterReadTemplate() };
            var v1 = new RegisterRead(new VirtualRegister());
            var v2 = new RegisterRead(new VirtualRegister());
            var v3 = new RegisterRead(new VirtualRegister());
            var node = new ArithmeticBinaryOperation(ArithmeticOperationType.Addition, v1, v2);
            var root = new ArithmeticBinaryOperation(ArithmeticOperationType.Addition, v3, node);
            Tree tree = new Tree(root);
            tree.ControlFlow = new Ret();
            InstructionSelector selector = new InstructionSelector(templates);
            var ins = selector.Select(tree) as List<Instruction>;
            Assert.AreEqual(6, ins.Count);
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
                VirtualRegister readFrom = fill[0] as VirtualRegister;

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
