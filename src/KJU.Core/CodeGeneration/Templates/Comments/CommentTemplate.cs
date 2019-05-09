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
    }
}