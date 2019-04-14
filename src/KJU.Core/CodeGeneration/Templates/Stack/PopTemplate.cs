#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Stack
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public class PopTemplate : InstructionTemplate
    {
        public PopTemplate()
            : base(new Pop(null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            return new PushInstruction(result);
        }

        private class PushInstruction : Instruction
        {
            private readonly VirtualRegister register;

            public PushInstruction(VirtualRegister register)
                : base(
                new List<VirtualRegister>(),
                new List<VirtualRegister> { register })
            {
                this.register = register;
            }

            public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var hardwareRegister = this.register.ToHardware(registerAssignment);
                return $"pop {hardwareRegister}{Environment.NewLine}";
            }
        }
    }
}