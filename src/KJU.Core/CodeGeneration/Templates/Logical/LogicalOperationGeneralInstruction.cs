#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AST;
    using Comparison;
    using Intermediate;

    public class LogicalOperationGeneralInstruction : Instruction
    {
        private readonly VirtualRegister lhs;
        private readonly VirtualRegister rhs;
        private readonly VirtualRegister result;
        private readonly LogicalBinaryOperationType operationType;

        public LogicalOperationGeneralInstruction(
            VirtualRegister lhs,
            VirtualRegister rhs,
            VirtualRegister result,
            LogicalBinaryOperationType operationType)
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
            this.operationType = operationType;
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

            builder.AppendLine($"{this.OperationTypeInstruction()} {resultHardware} {rhsHardware}");
            return builder.ToString();
        }
        
        private string OperationTypeInstruction()
        {
            switch (this.operationType)
            {
                case LogicalBinaryOperationType.And:
                    return "and";
                case LogicalBinaryOperationType.Or:
                    return "or";
                default:
                    throw new InstructionException("Something wrong with the type of the operation.");
            }
        }
    }
}