namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using Arithmetic;
    using Comparison;
    using Logical;
    using RawValues;
    using ReadWrite;
    using Stack;
    using Unary;

    public class InstructionsTemplatesFactory
    {
        private IReadOnlyList<InstructionTemplate> CreateInstructionTemplates()
        {
            var templates = new List<InstructionTemplate>
            {
                new RegisterWriteTemplate(),
                new RegisterReadTemplate(),
                new BooleanImmediateValueTemplate(),
                new IntegerImmediateValueTemplate(),
                new UnitImmediateValueTemplate(),
                new MemoryReadTemplate(),
                new MemoryWriteTemplate(),
                new AddTemplate(),
                new SubTemplate(),
                new MulTemplate(),
                new DivTemplate(),
                new ModTemplate(),

                new PushTemplate(),
                new PopTemplate()
            };

            var comparisonTemplates = new ComparisonTemplateFactory().GetTemplates();
            templates.AddRange(comparisonTemplates);

            var logicalOperationTemplates = new LogicalOperationTemplateFactory().GetTemplates();
            templates.AddRange(logicalOperationTemplates);

            var unaryOperationTemplates = new UnaryTemplateFactory().GetTemplates();
            templates.AddRange(unaryOperationTemplates);

            return templates;
        }
    }
}