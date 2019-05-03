namespace KJU.Core.CodeGeneration
{
    using System.Collections.Generic;
    using Intermediate;

    public class RetInstruction : Instruction
    {
        public RetInstruction()
            : base(
                HardwareRegisterUtils.CalleeSavedRegisters(),
                new List<HardwareRegister> { HardwareRegister.RSP })
        {
        }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return "ret";
        }
    }
}
