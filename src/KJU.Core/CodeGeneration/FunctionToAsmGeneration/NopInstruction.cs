namespace KJU.Core.CodeGeneration
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class NopInstruction : Instruction
    {
        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return string.Empty;
        }
    }
}