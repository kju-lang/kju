namespace KJU.Core.Intermediate
{
    using System.Collections.Generic;
    using System.Linq;

    public static class CFGUtils
    {
        public static ILabel MakeTreeChain(
            this IEnumerable<Node> nodes, ILabelFactory labelFactory, ControlFlowInstruction controlFlow)
        {
            var nodesList = nodes.Reverse().ToList();
            return MakeTreeChain(nodesList.Skip(1).Reverse(), labelFactory, labelFactory.GetLabel(new Tree(nodesList.First(), controlFlow)));
        }

        public static ILabel MakeTreeChain(this IEnumerable<Node> nodes, ILabelFactory labelFactory, ILabel after)
        {
            return nodes
                .Reverse()
                .Aggregate(after, (next, node) =>
                {
                    var controlFlow = new UnconditionalJump(next);
                    var tree = new Tree(node, controlFlow);
                    return labelFactory.GetLabel(tree);
                });
        }

        public static Node OffsetAddress(this VirtualRegister baseAddr, int offset)
        {
            return new ArithmeticBinaryOperation(
                AST.ArithmeticOperationType.Addition,
                new RegisterRead(baseAddr),
                new IntegerImmediateValue(offset * 8));
        }
    }
}