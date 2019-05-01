namespace KJU.Core.CodeGeneration
{
    using System.Collections.Generic;
    using Intermediate;

    public class CodeBlock
    {
        public CodeBlock(Label label, IReadOnlyList<Instruction> instructions)
        {
            this.Label = label;
            this.Instructions = instructions;
        }

        public Label Label { get; }

        public IReadOnlyList<Instruction> Instructions { get; }
    }
}