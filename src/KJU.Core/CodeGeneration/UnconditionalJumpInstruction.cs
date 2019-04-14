namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class UnconditionalJumpInstruction : Instruction
    {
        public Label Label { get; set; }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return $"jmp {this.Label.Id}\n";
        }
    }
}
