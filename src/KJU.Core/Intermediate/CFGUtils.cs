namespace KJU.Core.Intermediate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CFGUtils
    {
        public static ILabel MakeTreeChain(this IEnumerable<Node> nodes, ILabelFactory labelFactory, Tree after)
        {
            return MakeTreeChain(nodes, labelFactory, labelFactory.GetLabel(after));
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

        public static Node OffsetAddress(VirtualRegister baseAddr, int offset)
        {
            return new ArithmeticBinaryOperation(
                AST.ArithmeticOperationType.Addition,
                new RegisterRead(baseAddr),
                new IntegerImmediateValue(offset * 8));
        }

        public static Node RegisterCopy(VirtualRegister to, VirtualRegister from)
        {
            return new RegisterWrite(to, new RegisterRead(from));
        }
    }
}