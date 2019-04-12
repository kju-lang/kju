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

        public long Value { get; }

        public long? TemplateValue { get; set; }
    }

    public class BooleanImmediateValue : Node
    {
        public BooleanImmediateValue(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public bool? TemplateValue { get; set; }
    }

    public class UnitImmediateValue : Node
    {
    }

    public class MemoryRead : Node
    {
        public MemoryRead(Node addr)
        {
            this.Addr = addr;
        }

        public Node Addr { get; }
    }

    public class MemoryWrite : Node
    {
        public MemoryWrite(Node addr, Node value)
        {
            this.Addr = addr;
            this.Value = value;
        }

        public Node Addr { get; }

        public Node Value { get; }
    }

    public class RegisterRead : Node
    {
        public RegisterRead(VirtualRegister register)
        {
            this.Register = register;
        }

        public VirtualRegister Register { get; }
    }

    public class RegisterWrite : Node
    {
        public RegisterWrite(VirtualRegister register, Node value)
        {
            this.Register = register;
            this.Value = value;
        }

        public VirtualRegister Register { get; }

        public Node Value { get; }
    }

    public class LogicalBinaryOperation : Node
    {
        public LogicalBinaryOperation(Node lhs, Node rhs, AST.LogicalBinaryOperationType type)
        {
            this.Lhs = lhs;
            this.Rhs = rhs;
            this.Type = type;
        }

        public Node Lhs { get; }

        public Node Rhs { get; }

        public LogicalBinaryOperationType Type { get; }
    }

    public class ArithmeticBinaryOperation : Node
    {
        public ArithmeticBinaryOperation(ArithmeticOperationType type, Node lhs, Node rhs)
        {
            this.Type = type;
            this.Lhs = lhs;
            this.Rhs = rhs;
        }

        public Node Lhs { get; }

        public Node Rhs { get; }

        public ArithmeticOperationType Type { get; }
    }

    public class Comparison : Node
    {
        public Comparison(Node lhs, Node rhs, ComparisonType type)
        {
            this.Lhs = lhs;
            this.Rhs = rhs;
            this.Type = type;
        }

        public Node Lhs { get; }

        public Node Rhs { get; }

        public ComparisonType Type { get; }
    }

    public class UnaryOperation : Node
    {
        public UnaryOperation(Node operand, UnaryOperationType type)
        {
            this.Operand = operand;
            this.Type = type;
        }

        public Node Operand { get; }

        public UnaryOperationType Type { get; }
    }

    public class Push : Node
    {
        public Push(Node value)
        {
            this.Value = value;
        }

        public Node Value { get; }
    }

    public class Pop : Node
    {
        public Pop(VirtualRegister register)
        {
            this.Register = register;
        }

        public VirtualRegister Register { get; }
    }
}
