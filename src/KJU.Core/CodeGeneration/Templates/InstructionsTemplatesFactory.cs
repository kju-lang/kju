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
            return new List<InstructionTemplate>
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
                new AndTemplate(),

                new OrTemplate(),
                new EqualTemplate(),
                new NotEqualTemplate(),
                new LessTemplate(),
                new LessOrEqualTemplate(),
                new GreaterTemplate(),
                new GreaterOrEqualTemplate(),
                new MinusTemplate(),
                new NotTemplate(),
                new PlusTemplate(),
                new PushTemplate(),
                new PopTemplate()
            };
        }
    }
}