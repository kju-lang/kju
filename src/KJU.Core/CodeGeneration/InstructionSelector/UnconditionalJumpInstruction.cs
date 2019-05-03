namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class UnconditionalJumpInstruction : Instruction
    {
        public UnconditionalJumpInstruction(Label label)
        {
            this.Label = label;
        }

        public Label Label { get; }

        public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            yield return $"jmp {this.Label.Id}";
        }
    }
}