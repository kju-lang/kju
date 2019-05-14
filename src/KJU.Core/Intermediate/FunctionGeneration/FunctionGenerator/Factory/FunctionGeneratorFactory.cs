namespace KJU.Core.Intermediate.FunctionGeneration.FunctionGenerator.Factory
{
    using AST.VariableAccessGraph;
    using CodeGeneration.FunctionToAsmGeneration;
    using PrologueEpilogue;
    using ReadWrite;

    public class FunctionGeneratorFactory
    {
        public FunctionGenerator ConstructGenerator()
        {
            var labelIdGuidGenerator = new LabelIdGuidGenerator();
            var labelFactory = new LabelFactory(labelIdGuidGenerator);
            var variableAccessGraphGenerator = new VariableAccessGraphGeneratorFactory().GetGenerator();
            var temporaryVariablesExtractor = new TemporaryVariablesExtractor.TemporaryVariablesExtractor();
            var readWriteGenerator = new ReadWriteGenerator();
            var callingSiblingFinder = new CallingSiblingFinder.CallingSiblingFinder();
            var prologueEpilogueGenerator = new PrologueEpilogueGenerator(labelFactory, readWriteGenerator);
            var callGenerator = new CallGenerator.CallGenerator(labelFactory, callingSiblingFinder, readWriteGenerator);
            var functionBodyGenerator = new BodyGenerator.FunctionBodyGenerator(
                labelFactory,
                readWriteGenerator,
                prologueEpilogueGenerator,
                callGenerator);
            return new FunctionGenerator(
                variableAccessGraphGenerator,
                temporaryVariablesExtractor,
                functionBodyGenerator);
        }
    }
}