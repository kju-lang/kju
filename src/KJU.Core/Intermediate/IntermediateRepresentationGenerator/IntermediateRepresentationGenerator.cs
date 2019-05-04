namespace KJU.Core.Intermediate.IntermediateRepresentationGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using AST.Nodes;

    public class IntermediateRepresentationGenerator : IIntermediateRepresentationGenerator
    {
        public IReadOnlyDictionary<Function.Function, ILabel> CreateIR(AST.Node node)
        {
            return node
                .ChildrenRecursive()
                .OfType<AST.FunctionDeclaration>()
                .Where(fun => !fun.IsForeign)
                .ToDictionary(
                    decl => decl.IntermediateFunction,
                    decl => decl.IntermediateFunction.GenerateBody(decl));
        }
    }
}