#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Intermediate;

    public class LogicalBinaryOperationInstruction : Instruction
    {
        private readonly VirtualRegister lhs;
        private readonly VirtualRegister rhs;
        private readonly VirtualRegister result;
        private readonly string instruction;

        public LogicalBinaryOperationInstruction(
            VirtualRegister lhs,
            VirtualRegister rhs,
            VirtualRegister result,
            string instruction)
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
            this.instruction = instruction;
        }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            var lhsHardware = this.lhs.ToHardware(registerAssignment);
            var rhsHardware = this.rhs.ToHardware(registerAssignment);
            var resultHardware = this.result.ToHardware(registerAssignment);
            if (resultHardware == rhsHardware)
            {
                (lhsHardware, rhsHardware) = (rhsHardware, lhsHardware);
            }

            var builder = new StringBuilder();
            if (resultHardware != lhsHardware)
            {
                builder.AppendLine($"mov {resultHardware} {lhsHardware}");
            }

            builder.AppendLine($"{this.instruction} {resultHardware} {rhsHardware}");
            return builder.ToString();
        }
    }
}