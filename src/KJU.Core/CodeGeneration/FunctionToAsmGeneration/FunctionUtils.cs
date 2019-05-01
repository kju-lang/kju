namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Intermediate;

    internal static class FunctionUtils
    {
        internal static Label ConcatTrees(this IEnumerable<Node> trees, ControlFlowInstruction next)
        {
            var treesReversed = trees.Reverse().ToList();
            var resultTree = treesReversed
                .Skip(1)
                .Aggregate(
                    new Tree(treesReversed.First(), next),
                    (tree, node) => new Tree(node, new UnconditionalJump(new Label(tree))));
            return new Label(resultTree);
        }
    }
}