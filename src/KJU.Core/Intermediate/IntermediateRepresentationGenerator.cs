namespace KJU.Core.Intermediate
{
    using System.Collections.Generic;
    using System.Linq;
    using AST.Nodes;

    public class IntermediateRepresentationGenerator
    {
        public IReadOnlyDictionary<AST.FunctionDeclaration, Label> CreateIR(AST.Node node)
        {
            return node
                .ChildrenRecursive()
                .OfType<AST.FunctionDeclaration>()
                .Where(fun => !fun.IsForeign)
                .ToDictionary(
                    decl => decl,
                    decl =>
                        {
                            var generator = new FunctionBodyGenerator.FunctionBodyGenerator(decl.IntermediateFunction);
                            return generator.BuildFunctionBody(decl.Body);
                        });
        }
    }
}