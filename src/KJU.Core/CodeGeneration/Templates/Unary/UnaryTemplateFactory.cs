namespace KJU.Core.CodeGeneration.Templates.Unary
{
    using System.Collections.Generic;
    using AST;

    public class UnaryTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return new List<InstructionTemplate>
            {
                new UnaryGeneralTemplate(UnaryOperationType.Not),
                new UnaryGeneralTemplate(UnaryOperationType.Plus),
                new UnaryGeneralTemplate(UnaryOperationType.Minus)
            };
        }
    }
}