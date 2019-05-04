namespace KJU.Core.Intermediate.IntermediateRepresentationGenerator
{
    using System.Collections.Generic;

    public interface IIntermediateRepresentationGenerator
    {
        IReadOnlyDictionary<AST.FunctionDeclaration, ILabel> CreateIR(AST.Node node);
    }
}