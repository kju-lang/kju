namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Intermediate;

    internal static class FunctionUtils
    {
        internal static ILabel ConcatTrees(this IEnumerable<Node> trees, ILabelFactory labelFactory, ControlFlowInstruction next)
        {
            var treesReversed = trees.Reverse().ToList();
            var resultTree = treesReversed
                .Skip(1)
                .Aggregate(
                    new Tree(treesReversed.First(), next),
                    (tree, node) => new Tree(node, new UnconditionalJump(labelFactory.GetLabel(tree))));
            return labelFactory.GetLabel(resultTree);
        }
    }
}