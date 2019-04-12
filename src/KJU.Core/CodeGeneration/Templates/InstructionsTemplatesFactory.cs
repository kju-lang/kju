namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using Arithmetic;
    using RawValues;
    using ReadWrite;

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
                new ModTemplate()
            };
        }
    }
}