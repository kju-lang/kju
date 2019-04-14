namespace KJU.Core.CodeGeneration.Templates.Logical
{
    using System.Collections.Generic;
    using AST;

    public class LogicalOperationTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return new List<InstructionTemplate>
            {
                new LogicalOperationGeneralTemplate(LogicalBinaryOperationType.And),
                new LogicalOperationGeneralTemplate(LogicalBinaryOperationType.Or)
            };
        }
    }
}