namespace KJU.Core.CodeGeneration.InstructionSelector
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Intermediate;
    using KJU.Core.Intermediate.Function;

    public class CallInstruction : Instruction
    {
        public CallInstruction(Function function)
            : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                HardwareRegisterUtils.CallerSavedRegisters.Append(HardwareRegister.RSP).ToList())
        {
            this.Function = function;
        }

        public Function Function { get; }

        public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            yield return $"call {this.Function.MangledName}";
        }
    }
}
