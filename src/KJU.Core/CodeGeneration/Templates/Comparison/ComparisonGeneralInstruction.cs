namespace KJU.Core.CodeGeneration.Templates.Comparison
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;

    public class ComparisonGeneralInstruction : Instruction
    {
        private readonly VirtualRegister lhs;
        private readonly VirtualRegister rhs;
        private readonly VirtualRegister result;
        private readonly ComparisonType operationType;

        public ComparisonGeneralInstruction(
            VirtualRegister lhs,
            VirtualRegister rhs,
            VirtualRegister result,
            ComparisonType operationType)
            : base(
                new List<VirtualRegister> { lhs, rhs },
                new List<VirtualRegister> { result })
        {
            this.lhs = lhs;
            this.rhs = rhs;
            this.result = result;
            this.operationType = operationType;
        }

        public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            var lhsHardware = this.lhs.ToHardware(registerAssignment);
            var rhsHardware = this.rhs.ToHardware(registerAssignment);
            var resultHardware = this.result.ToHardware(registerAssignment);

            yield return $"cmp {lhsHardware}, {rhsHardware}";
            yield return $"{this.OperationTypeInstruction()} {resultHardware.ToEightBitsVersion()}";
            yield return $"and {resultHardware}, 1";
        }

        private string OperationTypeInstruction()
        {
            switch (this.operationType)
            {
                case ComparisonType.Equal:
                    return "sete";
                case ComparisonType.NotEqual:
                    return "setne";
                case ComparisonType.Less:
                    return "setl";
                case ComparisonType.LessOrEqual:
                    return "setle";
                case ComparisonType.Greater:
                    return "setg";
                case ComparisonType.GreaterOrEqual:
                    return "setge";
                default:
                    throw new InstructionException("Something wrong with the type of the operation.");
            }
        }
    }
}