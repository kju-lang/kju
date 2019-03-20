#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace KJU.Core.AST
{
    using System.Collections.Generic;

    public class Expression : Node
    {
        public DataType Type { get; set; }
    }

    public class Program : Node
    {
        public IReadOnlyList<FunctionDeclaration> Functions { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Functions);
        }
    }

    public class FunctionDeclaration : Expression
    {
        public string Identifier { get; set; }

        public DataType ReturnType { get; set; }

        public IReadOnlyList<VariableDeclaration> Parameters { get; set; }

        public InstructionBlock Body { get; set; }

        public override IEnumerable<Node> Children()
        {
            var result = new List<Node>();
            result.AddRange(this.Parameters);
            result.Add(this.Body);
            return result;
        }
    }

    public class InstructionBlock : Expression
    {
        public IReadOnlyList<Expression> Instructions { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Instructions);
        }
    }

    public class VariableDeclaration : Expression
    {
        public DataType VariableType { get; set; }

        public string Identifier { get; set; }

        public Expression Value { get; set; }

        public override IEnumerable<Node> Children()
        {
            if (this.Value == null)
                return new List<Node>();
            else
                return new List<Node>() { this.Value };
        }
    }

    public class WhileStatement : Expression
    {
        public Expression Condition { get; set; }

        public InstructionBlock Body { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>() { this.Condition, this.Body };
        }
    }

    public class IfStatement : Expression
    {
        public Expression Condition { get; set; }

        public InstructionBlock ThenBody { get; set; }

        public InstructionBlock ElseBody { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>() { this.Condition, this.ThenBody, this.ElseBody };
        }
    }

    public class FunctionCall : Expression
    {
        public string Function { get; set; }

        public IReadOnlyList<Expression> Arguments { get; set; }

        public FunctionDeclaration Declaration { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Arguments);
        }
    }

    public class ReturnStatement : Expression
    {
        public Expression Value { get; set; }

        public override IEnumerable<Node> Children()
        {
            if (this.Value == null)
                return new List<Node>();
            else
                return new List<Node>() { this.Value };
        }
    }

    public class Variable : Expression
    {
        public string Identifier { get; set; }

        public VariableDeclaration Declaration { get; set; }
    }

    public class BoolLiteral : Expression
    {
        public bool Value { get; set; }
    }

    public class IntegerLiteral : Expression
    {
        public long Value { get; set; }
    }

    public class Assignment : Expression
    {
        public Variable Lhs { get; set; }

        public Expression Value { get; set; }

        public override IEnumerable<Node> Children()
        {
            List<Node> result = new List<Node>() { this.Lhs };
            if (this.Value != null)
                result.Add(this.Value);
            return result;
        }
    }

    public class CompoundAssignment : Expression
    {
        public Variable Lhs { get; set; }

        public ArithmeticOperationType Operation { get; set; }

        public Expression Value { get; set; }

        public override IEnumerable<Node> Children()
        {
            List<Node> result = new List<Node>() { this.Lhs };
            if (this.Value != null)
                result.Add(this.Value);
            return result;
        }
    }

    public class ArithmeticOperation : Expression
    {
        public ArithmeticOperationType OperationType { get; set; }

        public Expression LeftValue { get; set; }

        public Expression RightValue { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>() { this.LeftValue, this.RightValue };
        }
    }

    public class Comparison : Expression
    {
        public ComparisonType OperationType { get; set; }

        public Expression LeftValue { get; set; }

        public Expression RightValue { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>() { this.LeftValue, this.RightValue };
        }
    }
}
