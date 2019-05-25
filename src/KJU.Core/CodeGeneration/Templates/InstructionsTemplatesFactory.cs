namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using Arithmetic;
    using Arithmetic.Addition;
    using Arithmetic.Multiplication;
    using Comments;
    using Comparison;
    using Logical;
    using RawValues;
    using ReadWrite;
    using Stack;
    using Unary;

    public class InstructionsTemplatesFactory
    {
        public IReadOnlyList<InstructionTemplate> CreateInstructionTemplates()
        {
            var powerOf2 = new PowerOf2MultiplicationTemplateFactory().GetTemplates();
            var multiplicationConstant = new MultiplicationByConstantTemplateFactory().GetTemplates();
            var addConstant = new AddConstantTemplateFactory().GetTemplates();
            var comparisonTemplates = new ComparisonTemplateFactory().GetTemplates();
            var logicalOperationTemplates = new LogicalOperationTemplateFactory().GetTemplates();
            var unaryOperationTemplates = new UnaryTemplateFactory().GetTemplates();
            var memoryWriteLeaTemplates = new MemoryWriteLeaTemplateFactory().GetTemplates();
            var memoryReadLeaTemplates = new MemoryReadLeaTemplateFactory().GetTemplates();
            var templates = new List<InstructionTemplate>
            {
                new RegisterWriteTemplate(),
                new RegisterReadTemplate(),
                new MemoryReadTemplate(),
                new MemoryWriteTemplate(),

                new BooleanImmediateValueTemplate(),
                new IntegerImmediateValueTemplate(),
                new UnitImmediateValueTemplate(),
                new GeneralAddTemplate(),
                new SubTemplate(),
                new GeneralMultiplicationTemplate(),
                new DivTemplate(),
                new ModTemplate(),

                new PushTemplate(),
                new PopTemplate(),
                new ReserveStackMemoryTemplate(),
                new AlignStackPointerTemplate(),
                new PushStackLayoutPointerTemplate(),
                new ClearDFTemplate(),

                new ConditionalJumpTemplate(),

                new CommentTemplate(),
                new UsesDefinesTemplate()
            };

            templates.AddRange(comparisonTemplates);
            templates.AddRange(logicalOperationTemplates);
            templates.AddRange(unaryOperationTemplates);
            templates.AddRange(powerOf2);
            templates.AddRange(multiplicationConstant);
            templates.AddRange(addConstant);
            templates.AddRange(memoryWriteLeaTemplates);
            templates.AddRange(memoryReadLeaTemplates);
            return templates;
        }
    }
}