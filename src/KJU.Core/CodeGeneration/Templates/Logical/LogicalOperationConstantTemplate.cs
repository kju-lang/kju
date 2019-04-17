#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;
    using LogicalBinaryOperation = Intermediate.LogicalBinaryOperation;

    public class LogicalOperationConstantTemplate : InstructionTemplate
    {
        private readonly LogicalBinaryOperationType operationType;
        private readonly bool leftConstant;

        public LogicalOperationConstantTemplate(
            LogicalBinaryOperationType operationType,
            BooleanImmediateValue constant,
            bool leftConstant)
            : base(
                new LogicalBinaryOperation(
                leftConstant ? constant : null,
                leftConstant ? null : constant,
                operationType), 2)
        {
            this.operationType = operationType;
            this.leftConstant = leftConstant;
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var constantPosition = 0;
            var registerPosition = 1;
            if (!this.leftConstant)
            {
                (constantPosition, registerPosition) = (registerPosition, constantPosition);
            }

            var constant = fill.GetBool(constantPosition);
            var register = fill.GetRegister(registerPosition);
            return new LogicalOperationConstantInstruction(constant, register, result, this.operationType);
        }
    }
}