namespace KJU.Core.Intermediate.IntermediateRepresentationGenerator
{
    using System.Collections.Generic;

    public interface IIntermediateRepresentationGenerator
    {
        IReadOnlyDictionary<AST.FunctionDeclaration, Label> CreateIR(AST.Node node);
    }
}