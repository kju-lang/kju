namespace KJU.Core.AST.VariableAccessGraph
{
    using System.Collections.Generic;

    public class AccessInfoExtractor : INodeInfoExtractor
    {
        public IEnumerable<VariableDeclaration> ExtractInfo(Node node)
        {
            switch (node)
            {
                case Variable variable:
                    return new List<VariableDeclaration>() { variable.Declaration };

                default:
                    return new List<VariableDeclaration>();
            }
        }
    }
}