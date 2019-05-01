namespace KJU.Core.CodeGeneration.Templates.Stack
{
    using System.Collections.Generic;
    using Intermediate;
    using KJU.Core.Intermediate.Function;

    public class ReserveStackMemoryTemplate : InstructionTemplate
    {
        public ReserveStackMemoryTemplate()
            : base(new ReserveStackMemory(null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            return new ReserveStackMemoryInstruction(fill.GetFunction(0));
        }

        private class ReserveStackMemoryInstruction : Instruction
        {
            public ReserveStackMemoryInstruction(Function fun)
                : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                new List<VirtualRegister> { HardwareRegister.RSP })
            {
                this.KjuFunction = fun;
            }

            private Function KjuFunction { get; }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var hardwareRegister = HardwareRegister.RSP;

                yield return $"sub {hardwareRegister}, {this.KjuFunction.StackBytes}";
            }
        }
    }
}
