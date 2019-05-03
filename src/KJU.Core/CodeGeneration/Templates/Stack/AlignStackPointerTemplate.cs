namespace KJU.Core.CodeGeneration.Templates.Stack
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class AlignStackPointerTemplate : InstructionTemplate
    {
        public AlignStackPointerTemplate()
            : base(new AlignStackPointer(false), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            return new AlignStackPointerInstruction(fill.GetBool(0));
        }

        private class AlignStackPointerInstruction : Instruction
        {
            private bool qwordOffset;

            public AlignStackPointerInstruction(bool qwordOffset)
                : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                new List<VirtualRegister> { HardwareRegister.RSP })
            {
                this.qwordOffset = qwordOffset;
            }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                yield return $"and {HardwareRegister.RSP}, -16";

                if (this.qwordOffset)
                {
                    yield return $"sub {HardwareRegister.RSP}, 8";
                }
            }
        }
    }
}
