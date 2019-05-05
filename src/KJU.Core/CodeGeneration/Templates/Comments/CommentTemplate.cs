namespace KJU.Core.CodeGeneration.Templates.Comments
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public class CommentTemplate : InstructionTemplate
    {
        public CommentTemplate()
            : base(new Comment(), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var value = fill.GetString(0);
            return new CommentInstruction(value);
        }

        private class CommentInstruction : Instruction
        {
            private readonly string value;

            public CommentInstruction(string value)
            {
                this.value = value;
            }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                yield return $"; {this.value}";
            }
        }
    }
}