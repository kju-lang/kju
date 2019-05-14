namespace KJU.Core.Intermediate.IntermediateRepresentationGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using AST.Nodes;
    using Function;
    using FunctionGeneration.FunctionGenerator;

    public class IntermediateRepresentationGenerator : IIntermediateRepresentationGenerator
    {
        private readonly FunctionGenerator functionGenerator;

        public IntermediateRepresentationGenerator(FunctionGenerator functionGenerator)
        {
            this.functionGenerator = functionGenerator;
        }

        public IReadOnlyDictionary<Function, ILabel> CreateIR(AST.Node node)
        {
            return node
                .ChildrenRecursive()
                .OfType<AST.FunctionDeclaration>()
                .Where(fun => !fun.IsForeign)
                .ToDictionary(
                    decl => decl.Function,
                    decl => this.functionGenerator.GenerateBody(decl.Function, decl));
        }
    }
}