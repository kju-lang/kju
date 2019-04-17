#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace KJU.Core.Intermediate
{
    using System.Collections.Generic;
    using KJU.Core.AST;

    public class Node
    {
        public virtual List<Node> Children()
        {
            return new List<Node>();
        }

        public virtual List<object> Match(Node template)
        {
            return null;
        }
    }

    public class IntegerImmediateValue : Node
    {
        public IntegerImmediateValue(long value)
        {
            this.Value = value;
        }

        public long Value { get; }

        public long? TemplateValue { get; set; }

        public override List<object> Match(Node template)
        {
            if (template is IntegerImmediateValue i)
            {
                if (i.TemplateValue == null)
                {
                    return new List<object> { this.Value };
                }

                if (i.TemplateValue == this.Value)
                {
                    return new List<object>();
                }

                return null;
            }

            return null;
        }
    }

    public class BooleanImmediateValue : Node
    {
        public BooleanImmediateValue()
        {
        }

        public BooleanImmediateValue(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public bool? TemplateValue { get; set; }

        public override List<object> Match(Node template)
        {
            if (template is BooleanImmediateValue b)
            {
                if (b.TemplateValue == null)
                {
                    return new List<object> { this.Value };
                }

                if (b.TemplateValue == this.Value)
                {
                    return new List<object>();
                }

                return null;
            }

            return null;
        }
    }

    public class UnitImmediateValue : Node
    {
        public override List<object> Match(Node template)
        {
            if (template is UnitImmediateValue)
            {
                return new List<object>();
            }

            return null;
        }
    }

    public class MemoryRead : Node
    {
        public MemoryRead(Node addr)
        {
            this.Addr = addr;
        }

        public Node Addr { get; }

        public override List<Node> Children()
        {
            return new List<Node> { this.Addr };
        }

        public override List<object> Match(Node template)
        {
            if (template is MemoryRead)
            {
                return new List<object>();
            }

            return null;
        }
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

        public override List<Node> Children()
        {
            return new List<Node> { this.Addr, this.Value };
        }

        public override List<object> Match(Node template)
        {
            if (template is MemoryWrite)
            {
                return new List<object>();
            }

            return null;
        }
    }

    public class RegisterRead : Node
    {
        public RegisterRead(VirtualRegister register)
        {
            this.Register = register;
        }

        public VirtualRegister Register { get; }

        public override List<object> Match(Node template)
        {
            if (template is RegisterRead)
            {
                return new List<object> { this.Register };
            }

            return null;
        }
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

        public override List<Node> Children()
        {
            return new List<Node> { this.Value };
        }

        public override List<object> Match(Node template)
        {
            if (template is RegisterWrite)
            {
                return new List<object> { this.Register };
            }

            return null;
        }
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

        public override List<Node> Children()
        {
            return new List<Node> { this.Lhs, this.Rhs };
        }

        public override List<object> Match(Node template)
        {
            if (template is LogicalBinaryOperation op)
            {
                if (op.Type.Equals(this.Type))
                {
                    return new List<object>();
                }
            }

            return null;
        }
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

        public override List<Node> Children()
        {
            return new List<Node> { this.Lhs, this.Rhs };
        }

        public override List<object> Match(Node template)
        {
            if (template is ArithmeticBinaryOperation op)
            {
                if (op.Type.Equals(this.Type))
                {
                    return new List<object>();
                }
            }

            return null;
        }
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

        public override List<Node> Children()
        {
            return new List<Node> { this.Lhs, this.Rhs };
        }

        public override List<object> Match(Node template)
        {
            if (template is Comparison com)
            {
                if (com.Type.Equals(this.Type))
                {
                    return new List<object>();
                }
            }

            return null;
        }
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

        public override List<Node> Children()
        {
            return new List<Node> { this.Operand };
        }

        public override List<object> Match(Node template)
        {
            if (template is UnaryOperation op)
            {
                if (op.Type.Equals(this.Type))
                {
                    return new List<object>();
                }
            }

            return null;
        }
    }

    public class Push : Node
    {
        public Push(Node value)
        {
            this.Value = value;
        }

        public Node Value { get; }

        public override List<Node> Children()
        {
            return new List<Node> { this.Value };
        }

        public override List<object> Match(Node template)
        {
            if (template is Push)
            {
                return new List<object>();
            }

            return null;
        }
    }

    public class Pop : Node
    {
        public Pop(VirtualRegister register)
        {
            this.Register = register;
        }

        public VirtualRegister Register { get; }

        public override List<object> Match(Node template)
        {
            if (template is Pop)
            {
                return new List<object> { this.Register };
            }

            return null;
        }
    }
}
