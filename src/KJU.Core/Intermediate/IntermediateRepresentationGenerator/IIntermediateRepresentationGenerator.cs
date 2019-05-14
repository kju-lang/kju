namespace KJU.Core.Intermediate.IntermediateRepresentationGenerator
{
    using System.Collections.Generic;
    using Function;

    public interface IIntermediateRepresentationGenerator
    {
        IReadOnlyDictionary<Function, ILabel> CreateIR(AST.Node node);
    }
}