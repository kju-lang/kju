namespace KJU.Core.CodeGeneration.Templates.Unary
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;
    using UnaryOperation = Intermediate.UnaryOperation;

    public class PlusTemplate : InstructionTemplate
    {
        public PlusTemplate()
            : base(new UnaryOperation(null, UnaryOperationType.Plus), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var input = fill.GetRegister(0);
            return new UnaryInstruction(input, result, UnaryOperationType.Plus);
        }
    }
}