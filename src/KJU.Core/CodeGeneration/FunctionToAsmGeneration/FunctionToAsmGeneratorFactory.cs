namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using LivenessAnalysis;
    using RegisterAllocation;

    public class FunctionToAsmGeneratorFactory
    {
        public IFunctionToAsmGenerator Generate()
        {
            var livenessAnalyzer = new LivenessAnalyzer();
            var registerAllocator = new RegisterAllocator();
            var instructionsTemplatesFactory = new Templates.InstructionsTemplatesFactory();
            var instructionTemplates = instructionsTemplatesFactory.CreateInstructionTemplates();
            var instructionSelector = new InstructionSelector.InstructionSelector(instructionTemplates);
            var labelIdGuidGenerator = new LabelIdGuidGenerator();
            var cfgLinearizer = new CFGLinearizer.CFGLinearizer();
            return new FunctionToAsmGenerator(
                livenessAnalyzer,
                registerAllocator,
                instructionSelector,
                labelIdGuidGenerator,
                cfgLinearizer);
        }
    }
}