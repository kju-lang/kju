namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Intermediate;

    public class RetInstruction : Instruction
    {
        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return "ret\n";
        }
    }
}
