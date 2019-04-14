namespace KJU.Core.CodeGeneration.Templates.Unary
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;
    using UnaryOperation = Intermediate.UnaryOperation;

    public class UnaryGeneralTemplate : InstructionTemplate
    {
        private readonly UnaryOperationType operationType;

        public UnaryGeneralTemplate(UnaryOperationType operationType)
            : base(new UnaryOperation(null, operationType), 1)
        {
            this.operationType = operationType;
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var input = fill.GetRegister(0);
            return new UnaryGeneralInstruction(input, result, this.operationType);
        }
    }
}