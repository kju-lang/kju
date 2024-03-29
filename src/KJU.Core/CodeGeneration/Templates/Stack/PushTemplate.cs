namespace KJU.Core.CodeGeneration.Templates.Stack
{
    using System.Collections.Generic;
    using Intermediate;

    public class PushTemplate : InstructionTemplate
    {
        public PushTemplate()
            : base(new Push(null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var input = fill.GetRegister(0);
            return new PushInstruction(input);
        }

        private class PushInstruction : Instruction
        {
            private readonly VirtualRegister register;

            public PushInstruction(VirtualRegister register)
                : base(
                    new List<VirtualRegister> { HardwareRegister.RSP, register },
                    new List<VirtualRegister> { HardwareRegister.RSP })
            {
                this.register = register;
            }

            public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var hardwareRegister = this.register.ToHardware(registerAssignment);
                yield return $"push {hardwareRegister}";
            }
        }
    }
}