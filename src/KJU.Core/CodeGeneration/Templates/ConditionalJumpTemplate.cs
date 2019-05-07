namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using Intermediate;

    public class ConditionalJumpTemplate : InstructionTemplate
    {
        public ConditionalJumpTemplate()
            : base(null, 1, true)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            if (label == null)
            {
                throw new TemplateCreationException("Label is null.");
            }

            var input = fill.GetRegister(0);
            return new ConditionalJumpInstruction(input, label);
        }

        private class ConditionalJumpInstruction : Instruction
        {
            private readonly VirtualRegister register;
            private readonly string label;

            public ConditionalJumpInstruction(VirtualRegister register, string label)
                : base(new List<VirtualRegister> { register })
            {
                this.register = register;
                this.label = label;
            }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var hardwareRegister = this.register.ToHardware(registerAssignment);

                yield return $"test {hardwareRegister}, {hardwareRegister}";
                yield return $"jnz {this.label}";
            }
        }
    }
}