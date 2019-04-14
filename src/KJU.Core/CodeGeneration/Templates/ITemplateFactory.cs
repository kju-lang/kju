namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;

    public interface ITemplateFactory
    {
        IList<InstructionTemplate> GetTemplates();
    }
}
