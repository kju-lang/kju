namespace KJU.Core.CodeGeneration
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class NopInstruction : Instruction
    {
        public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            yield break;
        }
    }
}