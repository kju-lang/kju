namespace KJU.Core.CodeGeneration
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public abstract class InstructionTemplate
    {
        public Node Shape { get; set; }

        public bool IsConditionalJump { get; set; }

        public float Score { get; set; }

        public abstract Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label);
    }
}
