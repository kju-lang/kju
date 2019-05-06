namespace KJU.Core.AST.VariableAccessGraph
{
    using System.Collections.Generic;

    public class ModifyInfoExtractor : INodeInfoExtractor
    {
        public IEnumerable<VariableDeclaration> ExtractInfo(Node node)
        {
            switch (node)
            {
                case Assignment assignment:
                    return new List<VariableDeclaration>() { assignment.Lhs.Declaration };

                case CompoundAssignment compoundAssignment:
                    return new List<VariableDeclaration>() { compoundAssignment.Lhs.Declaration };

                default:
                    return new List<VariableDeclaration>();
            }
        }
    }
}