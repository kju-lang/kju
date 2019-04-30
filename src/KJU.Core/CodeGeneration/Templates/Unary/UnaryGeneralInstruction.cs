#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Unary
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AST;
    using Intermediate;

    public class UnaryGeneralInstruction : Instruction
    {
        private readonly VirtualRegister input;
        private readonly VirtualRegister result;
        private readonly UnaryOperationType type;

        public UnaryGeneralInstruction(
            VirtualRegister input,
            VirtualRegister result,
            UnaryOperationType type)
            : base(
                new List<VirtualRegister> { input },
                new List<VirtualRegister> { result },
                new List<Tuple<VirtualRegister, VirtualRegister>>
                {
                    new Tuple<VirtualRegister, VirtualRegister>(input, result)
                })
        {
            this.input = input;
            this.result = result;
            this.type = type;
        }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            var inputHardware = this.input.ToHardware(registerAssignment);
            var resultHardware = this.input.ToHardware(registerAssignment);

            var builder = new StringBuilder();
            if (inputHardware != resultHardware)
                builder.AppendLine($"mov {resultHardware} {inputHardware}");

            switch (this.type)
            {
                case UnaryOperationType.Not:
                    builder.AppendLine($"xor {resultHardware} 1");
                    break;
                case UnaryOperationType.Minus:
                    builder.AppendLine($"neg {resultHardware}");
                    break;
            }

            return builder.ToString();
        }
    }
}