namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;

    public partial class InstructionsTemplatesFactory
    {
        private IReadOnlyList<InstructionTemplate> CreateInstructionTemplates()
        {
            return new List<InstructionTemplate> { };
        }
    }
}