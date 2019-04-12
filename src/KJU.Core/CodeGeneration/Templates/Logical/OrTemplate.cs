namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;
    using LogicalBinaryOperation = Intermediate.LogicalBinaryOperation;

    public class OrTemplate : InstructionTemplate
    {
        public OrTemplate()
            : base(new LogicalBinaryOperation(null, null, LogicalBinaryOperationType.Or), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new LogicalBinaryOperationInstruction(lhs, rhs, result, "or");
        }
    }
}