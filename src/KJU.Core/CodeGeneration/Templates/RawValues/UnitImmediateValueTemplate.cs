namespace KJU.Core.CodeGeneration.Templates.RawValues
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public class UnitImmediateValueTemplate : InstructionTemplate
    {
        public UnitImmediateValueTemplate()
            : base(new UnitImmediateValue(), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            return new UnitImmediateValueInstruction();
        }

        private class UnitImmediateValueInstruction : Instruction
        {
            public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                yield break;
            }
        }
    }
}