#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Arithmetic
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AST;
    using Intermediate;

    internal class ModTemplate : InstructionTemplate
    {
        public ModTemplate()
            : base(new ArithmeticBinaryOperation(ArithmeticOperationType.Remainder, null, null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new ModInstruction(lhs, rhs, result);
        }

        private class ModInstruction : Instruction
        {
            private readonly VirtualRegister lhs;
            private readonly VirtualRegister rhs;
            private readonly VirtualRegister result;

            public ModInstruction(
                VirtualRegister lhs,
                VirtualRegister rhs,
                VirtualRegister result)
                : base(
                    new List<VirtualRegister> { lhs, rhs },
                    new List<VirtualRegister>
                    {
                        result,
                        HardwareRegister.RAX,
                        HardwareRegister.RDX
                    }, new List<Tuple<VirtualRegister, VirtualRegister>>
                    {
                        new Tuple<VirtualRegister, VirtualRegister>(HardwareRegister.RAX, lhs),
                    })
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
                var rax = HardwareRegister.RAX;
                var rdx = HardwareRegister.RDX;
                var builder = new StringBuilder();
                if (rax != lhsHardware)
                {
                    builder.AppendLine($"mov {rax}, {lhsHardware}");
                }

                builder.AppendLine($"mov {rdx}, 0");

                builder.Append($"idiv {rhsHardware}");
                if (rdx != resultHardware)
                {
                    builder.AppendLine();
                    builder.Append($"mov {resultHardware}, {rdx}");
                }

                return builder.ToString();
            }
        }
    }
}