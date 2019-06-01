namespace KJU.Core.CodeGeneration.Templates.RawValues
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    internal class LabelImmediateValueTemplate : InstructionTemplate
    {
        public LabelImmediateValueTemplate()
            : base(new LabelImmediateValue() { TemplateValue = null }, 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var value = fill.GetString(0);
            return new LabelImmediateValueInstruction(result, value);
        }

        private class LabelImmediateValueInstruction : Instruction
        {
            private readonly VirtualRegister result;
            private readonly string value;

            public LabelImmediateValueInstruction(
                VirtualRegister result,
                string value)
                : base(
                    new List<VirtualRegister>(),
                    new List<VirtualRegister> { result })
            {
                this.result = result;
                this.value = value;
            }

            public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var writeTo = this.result.ToHardware(registerAssignment);
                yield return $"mov {writeTo}, {this.value}";
            }
        }
    }
}
