namespace KJU.Core.Intermediate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CFGUtils
    {
        public static Label MakeTreeChain(IEnumerable<Node> nodes, Tree after)
        {
            return MakeTreeChain(nodes, new Label(after));
        }

        public static Label MakeTreeChain(IEnumerable<Node> nodes, Label after)
        {
            Func<Label, Node, Label> linkToNext = (next, node) =>
                new Label(new Tree(node, new UnconditionalJump(next)));

            return nodes.Reverse().Aggregate(after, linkToNext);
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
