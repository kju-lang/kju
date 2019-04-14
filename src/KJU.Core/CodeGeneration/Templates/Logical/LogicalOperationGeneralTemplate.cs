namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System.Collections.Generic;
    using AST;
    using Comparison;
    using Intermediate;
    using LogicalBinaryOperation = Intermediate.LogicalBinaryOperation;

    public class LogicalOperationGeneralTemplate : InstructionTemplate
    {
        private readonly LogicalBinaryOperationType operationType;

        public LogicalOperationGeneralTemplate(LogicalBinaryOperationType operationType)
            : base(new LogicalBinaryOperation(null, null, operationType), 1)
        {
            this.operationType = operationType;
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new LogicalOperationGeneralInstruction(lhs, rhs, result, this.operationType);
        }
    }
}