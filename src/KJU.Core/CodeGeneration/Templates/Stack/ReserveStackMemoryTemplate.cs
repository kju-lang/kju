namespace KJU.Core.CodeGeneration.Templates.Stack
{
    using System.Collections.Generic;
    using Intermediate;
    using Intermediate.Function;

    public class ReserveStackMemoryTemplate : InstructionTemplate
    {
        public ReserveStackMemoryTemplate()
            : base(new ReserveStackMemory(null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var functionInfo = fill.GetFunctionInfo(0);
            return new ReserveStackMemoryInstruction(functionInfo);
        }

        private class ReserveStackMemoryInstruction : Instruction
        {
            private readonly Function function;

            public ReserveStackMemoryInstruction(Function function)
                : base(
                    new List<VirtualRegister> { HardwareRegister.RSP },
                    new List<VirtualRegister> { HardwareRegister.RSP })
            {
                this.function = function;
            }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                int stackBytes = this.function.StackBytes;

                // always pad stack to 16 bytes
                if (stackBytes % 16 == 8)
                {
                    stackBytes += 8;
                }

                yield return $"sub {HardwareRegister.RSP}, {stackBytes}";
            }
        }
    }
}