namespace KJU.Core.Intermediate.IntermediateRepresentationGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using AST.Nodes;

    public class IntermediateRepresentationGenerator : IIntermediateRepresentationGenerator
    {
        public IReadOnlyDictionary<AST.FunctionDeclaration, ILabel> CreateIR(AST.Node node)
        {
            return node
                .ChildrenRecursive()
                .OfType<AST.FunctionDeclaration>()
                .Where(fun => !fun.IsForeign)
                .ToDictionary(
                    decl => decl,
                    decl =>
                        {
                            return decl.IntermediateFunction.GenerateBody(decl);
                        });
        }
    }
}