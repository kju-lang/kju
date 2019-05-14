namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using Intermediate;
    using Intermediate.FunctionGeneration.ReadWrite;
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
            var cfgLinearizer = new CfgLinearizer.CfgLinearizer();
            var labelFactory = new LabelFactory(new LabelIdGuidGenerator());
            var readWriteGenerator = new ReadWriteGenerator();
            return new FunctionToAsmGenerator(
                livenessAnalyzer,
                registerAllocator,
                instructionSelector,
                cfgLinearizer,
                labelFactory,
                readWriteGenerator);
        }
    }
}