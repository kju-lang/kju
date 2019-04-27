namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Intermediate;

    public class RetInstruction : Instruction
    {
        public RetInstruction()
            : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                new List<VirtualRegister> { HardwareRegister.RSP })
        {
        }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return "ret\n";
        }
    }
}
