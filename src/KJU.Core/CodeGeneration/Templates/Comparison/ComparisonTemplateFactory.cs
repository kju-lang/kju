namespace KJU.Core.CodeGeneration.Templates.Comparison
{
    using System.Collections.Generic;
    using AST;

    public class ComparisonTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return new List<InstructionTemplate>
            {
                new ComparisonGeneralTemplate(ComparisonType.Equal),
                new ComparisonGeneralTemplate(ComparisonType.NotEqual),
                new ComparisonGeneralTemplate(ComparisonType.Less),
                new ComparisonGeneralTemplate(ComparisonType.LessOrEqual),
                new ComparisonGeneralTemplate(ComparisonType.Greater),
                new ComparisonGeneralTemplate(ComparisonType.GreaterOrEqual)
            };
        }
    }
}