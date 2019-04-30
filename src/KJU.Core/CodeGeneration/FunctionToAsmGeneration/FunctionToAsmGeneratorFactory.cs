namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using LivenessAnalysis;
    using RegisterAllocation;

    public class FunctionToAsmGeneratorFactory
    {
        public FunctionToAsmGenerator Generate()
        {
            var livenessAnalyzer = new LivenessAnalyzer();
            var registerAllocator = new RegisterAllocator();
            var instructionsTemplatesFactory = new CodeGeneration.Templates.InstructionsTemplatesFactory();
            var instructionTemplates = instructionsTemplatesFactory.CreateInstructionTemplates();
            var instructionSelector = new InstructionSelector.InstructionSelector(instructionTemplates);
            var labelIdGuidGenerator = new LabelIdGuidGenerator();
            return new FunctionToAsmGenerator(
                livenessAnalyzer,
                registerAllocator,
                instructionSelector,
                labelIdGuidGenerator);
        }
    }
}