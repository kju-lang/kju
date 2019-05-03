namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System;

    public class LabelIdGuidGenerator : ILabelIdGenerator
    {
        public string GenerateLabelId()
        {
            return $".{Guid.NewGuid():N}";
        }
    }
}