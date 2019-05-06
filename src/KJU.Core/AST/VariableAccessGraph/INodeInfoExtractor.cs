namespace KJU.Core.AST.VariableAccessGraph
{
    using System.Collections.Generic;

    public interface INodeInfoExtractor
    {
        IEnumerable<VariableDeclaration> ExtractInfo(Node node);
    }
}