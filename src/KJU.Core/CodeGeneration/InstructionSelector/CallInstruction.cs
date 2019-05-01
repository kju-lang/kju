namespace KJU.Core.CodeGeneration
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;

    public class CallInstruction : Instruction
    {
        public CallInstruction(Function function)
            : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                HardwareRegisterUtils.CallerSavedRegisters().Append(HardwareRegister.RSP).ToList())
        {
            this.Function = function;
        }

        public Function Function { get; }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return $"call {this.Function.MangledName}\n";
        }
    }
}
