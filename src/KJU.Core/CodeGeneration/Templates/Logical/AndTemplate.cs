namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;
    using LogicalBinaryOperation = KJU.Core.Intermediate.LogicalBinaryOperation;

    public class AndTemplate : InstructionTemplate
    {
        public AndTemplate()
            : base(new LogicalBinaryOperation(null, null, LogicalBinaryOperationType.And), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new LogicalBinaryOperationInstruction(lhs, rhs, result, "and");
        }
    }
}