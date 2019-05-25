namespace KJU.Core.CodeGeneration.Templates.Stack
{
    using System.Collections.Generic;
    using Intermediate;
    using Intermediate.Function;

    public class PushStackLayoutPointerTemplate : InstructionTemplate
    {
        public PushStackLayoutPointerTemplate()
            : base(new PushStackLayoutPointer(null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            return new PushStackLayoutPointerInstruction(fill.GetFunctionInfo(0));
        }

        private class PushStackLayoutPointerInstruction : Instruction
        {
            private readonly Function function;

            public PushStackLayoutPointerInstruction(Function function)
                : base(
                    new List<VirtualRegister> { HardwareRegister.RSP },
                    new List<VirtualRegister> { HardwareRegister.RSP })
            {
                this.function = function;
            }

            public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                yield return $"push {this.function.LayoutLabel}";
            }
        }
    }
}