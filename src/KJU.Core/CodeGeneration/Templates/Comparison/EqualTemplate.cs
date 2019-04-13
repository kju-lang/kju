#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Comparison
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AST;
    using Intermediate;

    public class EqualTemplate : InstructionTemplate
    {
        public EqualTemplate()
            : base(new Intermediate.Comparison(null, null, ComparisonType.Equal), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new EqualInstruction(lhs, rhs, result);
        }

        private class EqualInstruction : Instruction
        {
            private readonly VirtualRegister lhs;
            private readonly VirtualRegister rhs;
            private readonly VirtualRegister result;

            public EqualInstruction(
                VirtualRegister lhs,
                VirtualRegister rhs,
                VirtualRegister result)
                : base(
                    new List<VirtualRegister> { lhs, rhs },
                    new List<VirtualRegister> { result },
                    new List<Tuple<VirtualRegister, VirtualRegister>>
                    {
                        new Tuple<VirtualRegister, VirtualRegister>(lhs, result),
                        new Tuple<VirtualRegister, VirtualRegister>(rhs, result)
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
                var builder = new StringBuilder();

                if (resultHardware == rhsHardware)
                {
                    (lhsHardware, rhsHardware) = (rhsHardware, lhsHardware);
                }

                if (resultHardware != lhsHardware)
                {
                    builder.AppendLine($"mov {resultHardware} {lhsHardware}");
                }

                builder.AppendLine($"xor {resultHardware} {rhsHardware}");
                builder.AppendLine($"sete {resultHardware}");
                return builder.ToString();
            }
        }
    }
}