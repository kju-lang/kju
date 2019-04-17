namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;

    public class LogicalOperationTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return new List<InstructionTemplate>
            {
                new LogicalOperationGeneralTemplate(LogicalBinaryOperationType.And),
                new LogicalOperationGeneralTemplate(LogicalBinaryOperationType.Or),
                new LogicalOperationConstantTemplate(LogicalBinaryOperationType.And, new BooleanImmediateValue(), true),
                new LogicalOperationConstantTemplate(LogicalBinaryOperationType.And, new BooleanImmediateValue(), false),
                new LogicalOperationConstantTemplate(LogicalBinaryOperationType.Or, new BooleanImmediateValue(), true),
                new LogicalOperationConstantTemplate(LogicalBinaryOperationType.Or, new BooleanImmediateValue(), false),
            };
        }
    }
}