namespace KJU.Core.Intermediate.IntermediateRepresentationGenerator
{
    using System.Collections.Generic;

    public interface IIntermediateRepresentationGenerator
    {
        IReadOnlyDictionary<Function.Function, ILabel> CreateIR(AST.Node node);
    }
}