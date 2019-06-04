namespace KJU.Core.CodeGeneration.InstructionSelector
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Intermediate.Function;

    public class ComputedCallInstruction : Instruction
    {
        public ComputedCallInstruction()
            : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                HardwareRegisterUtils.CallerSavedRegisters.Append(HardwareRegister.RSP).ToList())
        {
        }

        public override IEnumerable<string> ToASM(
            IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            yield return $"call RAX";
        }
    }
}
