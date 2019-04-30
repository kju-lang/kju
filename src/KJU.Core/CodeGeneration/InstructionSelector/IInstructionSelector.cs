namespace KJU.Core.CodeGeneration.InstructionSelector
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public interface IInstructionSelector
    {
        IEnumerable<Instruction> GetInstructions(Tree tree);
    }
}