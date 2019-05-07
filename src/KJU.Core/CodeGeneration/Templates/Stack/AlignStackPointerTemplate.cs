namespace KJU.Core.CodeGeneration.Templates.Stack
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class AlignStackPointerTemplate : InstructionTemplate
    {
        public AlignStackPointerTemplate()
            : base(new AlignStackPointer(0), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            return new AlignStackPointerInstruction((int)fill[0]);
        }

        private class AlignStackPointerInstruction : Instruction
        {
            private int offset;

            public AlignStackPointerInstruction(int offset)
                : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                new List<VirtualRegister> { HardwareRegister.RSP })
            {
                this.offset = offset;
            }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                if (this.offset != 0)
                    yield return $"sub {HardwareRegister.RSP}, {this.offset}";
            }
        }
    }
}
