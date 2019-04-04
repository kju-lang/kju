#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace KJU.Core.Intermediate
{
    using KJU.Core.AST;

    public class Node
    {
    }

    public class IntegerImmediateValue : Node
    {
        public long Value { get; set; }
    }

    public class BooleanImmediateValue : Node
    {
        public bool Value { get; set; }
    }

    public class MemoryRead : Node
    {
        public Node Addr { get; set; }
    }

    public class MemoryWrite : Node
    {
        public Node Addr { get; set; }

        public Node Value { get; set; }
    }

    public class RegisterRead : Node
    {
        public VirtualRegister Register { get; set; }
    }

    public class RegisterWrite : Node
    {
        public VirtualRegister Register { get; set; }

        public Node Value { get; set; }
    }

    public class LogicalBinaryOperation : Node
    {
        public Node Lhs { get; set; }

        public Node Rhs { get; set; }

        public LogicalBinaryOperationType Type { get; set; }
    }

    public class ArithmeticBinaryOperation : Node
    {
        public Node Lhs { get; set; }

        public Node Rhs { get; set; }

        public ArithmeticOperationType Type { get; set; }
    }

    public class Comparison : Node
    {
        public Node Lhs { get; set; }

        public Node Rhs { get; set; }

        public ComparisonType Type { get; set; }
    }

    public class UnaryOperation : Node
    {
        public Node Operand { get; set; }

        public UnaryOperationType Type { get; set; }
    }
}