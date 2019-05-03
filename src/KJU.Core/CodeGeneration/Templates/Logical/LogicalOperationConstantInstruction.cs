#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System;
    using System.Collections.Generic;
    using AST;
    using Comparison;
    using Intermediate;

    public class LogicalOperationConstantInstruction : Instruction
    {
        private readonly VirtualRegister register;
        private readonly bool constant;
        private readonly VirtualRegister result;
        private readonly LogicalBinaryOperationType operationType;

        public LogicalOperationConstantInstruction(
            bool constant,
            VirtualRegister register,
            VirtualRegister result,
            LogicalBinaryOperationType operationType)
            : base(
                new List<VirtualRegister> { register },
                new List<VirtualRegister> { result },
                new List<Tuple<VirtualRegister, VirtualRegister>>
                {
                    new Tuple<VirtualRegister, VirtualRegister>(register, result)
                })
        {
            this.constant = constant;
            this.register = register;
            this.result = result;
            this.operationType = operationType;
        }

        public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            var registerHardware = this.register.ToHardware(registerAssignment);
            var resultHardware = this.result.ToHardware(registerAssignment);

            switch (this.operationType)
            {
                case LogicalBinaryOperationType.Or:
                    yield return this.constant
                        ? $"mov {resultHardware}, 1"
                        : $"mov {resultHardware}, {registerHardware}";
                    break;
                case LogicalBinaryOperationType.And:
                    yield return !this.constant
                        ? $"mov {resultHardware}, 0"
                        : $"mov {resultHardware}, {registerHardware}";
                    break;
                default:
                    throw new InstructionException("Something wrong with the type of the operation.");
            }
        }
    }
}