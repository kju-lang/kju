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

        public class ConditionalJumpInstruction : Instruction
        {
            private readonly VirtualRegister register;

            public ConditionalJumpInstruction(VirtualRegister register, string label)
                : base(new List<VirtualRegister> { register })
            {
                this.register = register;
                this.Label = label;
            }

            public string Label { get; }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var hardwareRegister = this.register.ToHardware(registerAssignment);

                yield return $"test {hardwareRegister}, {hardwareRegister}";
                yield return $"jnz {this.Label}";
            }
        }
    }
}