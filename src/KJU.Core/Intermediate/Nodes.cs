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
        public IntegerImmediateValue(long value)
        {
            this.Value = value;
        }

        public long Value { get; set; }
    }

    public class BooleanImmediateValue : Node
    {
        public bool Value { get; set; }
    }

    public class MemoryRead : Node
    {
        public MemoryRead(Node addr)
        {
            this.Addr = addr;
        }

        public Node Addr { get; set; }
    }

    public class MemoryWrite : Node
    {
        public MemoryWrite(Node addr, Node value)
        {
            this.Addr = addr;
            this.Value = value;
        }

        public Node Addr { get; set; }

        public Node Value { get; set; }
    }

    public class RegisterRead : Node
    {
        public RegisterRead(VirtualRegister register)
        {
            this.Register = register;
        }

        public VirtualRegister Register { get; set; }
    }

    public class RegisterWrite : Node
    {
        public RegisterWrite(VirtualRegister register, Node value)
        {
            this.Register = register;
            this.Value = value;
        }

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
        public ArithmeticBinaryOperation(ArithmeticOperationType type, Node lhs, Node rhs)
        {
            this.Type = type;
            this.Lhs = lhs;
            this.Rhs = rhs;
        }

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