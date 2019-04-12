namespace KJU.Core.CodeGeneration.Templates.Unary
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;
    using UnaryOperation = Intermediate.UnaryOperation;

    public class NotTemplate : InstructionTemplate
    {
        public NotTemplate()
            : base(new UnaryOperation(null, UnaryOperationType.Not), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var input = fill.GetRegister(0);
            return new UnaryInstruction(input, result, UnaryOperationType.Not);
        }
    }
}