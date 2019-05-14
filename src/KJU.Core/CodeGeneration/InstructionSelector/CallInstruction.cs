namespace KJU.Core.CodeGeneration.InstructionSelector
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Intermediate.Function;

    public class CallInstruction : Instruction
    {
        private readonly Function function;

        public CallInstruction(Function function)
            : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                HardwareRegisterUtils.CallerSavedRegisters.Append(HardwareRegister.RSP).ToList())
        {
            this.function = function;
        }

        public override IEnumerable<string> ToASM(
            IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            if (this.function.IsForeign)
            {
                yield return $"extern {this.function.MangledName}";
            }

            yield return $"call {this.function.MangledName}";
        }
    }
}