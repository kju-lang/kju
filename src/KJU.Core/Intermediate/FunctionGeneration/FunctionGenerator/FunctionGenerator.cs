namespace KJU.Core.Intermediate.FunctionGeneration.FunctionGenerator
{
    using System.Linq;
    using AST.VariableAccessGraph;
    using BodyGenerator;
    using TemporaryVariablesExtractor;

    public class FunctionGenerator
    {
        private readonly IVariableAccessGraphGenerator variableAccessGraphGenerator;
        private readonly FunctionBodyGenerator functionBodyGenerator;
        private readonly TemporaryVariablesExtractor temporaryVariablesExtractor;

        public FunctionGenerator(
            IVariableAccessGraphGenerator variableAccessGraphGenerator,
            TemporaryVariablesExtractor temporaryVariablesExtractor,
            FunctionBodyGenerator bodyGenerator)
        {
            this.variableAccessGraphGenerator = variableAccessGraphGenerator;
            this.temporaryVariablesExtractor = temporaryVariablesExtractor;
            this.functionBodyGenerator = bodyGenerator;
        }

        public ILabel GenerateBody(Function.Function function, AST.FunctionDeclaration root)
        {
            this.ExtractTemporaryVariables(root);
            return this.functionBodyGenerator.BuildFunctionBody(function, root.Body);
        }

        private void ExtractTemporaryVariables(AST.FunctionDeclaration root)
        {
            var variableAccess = this.variableAccessGraphGenerator.GetVariableInfoPerAstNode(root);
            var result = this.temporaryVariablesExtractor.ExtractTemporaryVariables(variableAccess, root.Body);
            var instructions = result.Concat(root.Body.Instructions).ToList();
            root.Body = new AST.InstructionBlock(root.Body.InputRange, instructions);
        }
    }
}