namespace KJU.Core.CodeGeneration.Templates.RawValues
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public class BooleanImmediateValueTemplate : InstructionTemplate
    {
        public BooleanImmediateValueTemplate()
            : base(new BooleanImmediateValue(false) { TemplateValue = null }, 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var value = fill.GetBool(0);
            return new BooleanImmediateValueInstruction(result, value);
        }

        private class BooleanImmediateValueInstruction : Instruction
        {
            private readonly VirtualRegister result;
            private readonly bool value;

            public BooleanImmediateValueInstruction(
                VirtualRegister result,
                bool value)
                : base(
                    new List<VirtualRegister>(),
                    new List<VirtualRegister> { result },
                    new List<Tuple<VirtualRegister, VirtualRegister>>())
            {
                this.result = result;
                this.value = value;
            }

            public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var writeTo = this.result.ToHardware(registerAssignment);
                var valueInt = this.value ? 1 : 0;
                yield return $"mov {writeTo}, {valueInt}";
            }
        }
    }
}