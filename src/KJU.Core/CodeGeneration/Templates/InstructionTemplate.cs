namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public abstract class InstructionTemplate
    {
        protected InstructionTemplate(Node shape, float score, bool isConditionalJump = false)
        {
            this.Shape = shape;
            this.IsConditionalJump = isConditionalJump;
            this.Score = score;
        }

        public Node Shape { get; }

        public bool IsConditionalJump { get; }

        public float Score { get; }

        public abstract Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label);
    }
}