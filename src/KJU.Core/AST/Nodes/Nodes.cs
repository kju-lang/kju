#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace KJU.Core.AST
{
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;

    public class Expression : Node
    {
        public DataType Type { get; set; }
    }

    public class Program : Node
    {
        public Program(IReadOnlyList<FunctionDeclaration> functions)
        {
            this.Functions = functions;
        }

        public IReadOnlyList<FunctionDeclaration> Functions { get; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Functions);
        }
    }

    public class FunctionDeclaration : Expression
    {
        public FunctionDeclaration(
            string identifier,
            DataType returnType,
            IReadOnlyList<VariableDeclaration> parameters,
            InstructionBlock body)
        {
            this.Identifier = identifier;
            this.ReturnType = returnType;
            this.Parameters = parameters;
            this.Body = body;
        }

        public string Identifier { get; }

        public DataType ReturnType { get; set; }

        public IReadOnlyList<VariableDeclaration> Parameters { get; }

        public InstructionBlock Body { get; set; }

        public Intermediate.Function IntermediateFunction { get; set; }

        public static bool ParametersTypesEquals(FunctionDeclaration left, FunctionDeclaration right)
        {
            return ParametersTypesEquals(
                left.Parameters.Select(x => x.VariableType).ToList(),
                right.Parameters.Select(x => x.VariableType).ToList());
        }

        public static bool ParametersTypesEquals(List<DataType> left, List<DataType> right)
        {
            return left.SequenceEqual(right);
        }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Parameters) { this.Body };
        }

        public override string ToString()
        {
            return
                $"{this.ReturnType} {this.Identifier}({string.Join(", ", this.Parameters.Select(x => x.VariableType))})";
        }
    }

    public class InstructionBlock : Expression
    {
        public InstructionBlock(IReadOnlyList<Expression> instructions)
        {
            this.Instructions = instructions;
        }

        public IReadOnlyList<Expression> Instructions { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Instructions);
        }
    }

    public class VariableDeclaration : Expression
    {
        public VariableDeclaration(DataType variableType, string identifier, Expression value)
        {
            this.VariableType = variableType;
            this.Identifier = identifier;
            this.Value = value;
        }

        public DataType VariableType { get; set; }

        public string Identifier { get; }

        public Expression Value { get; }

        public Intermediate.Variable IntermediateVariable { get; set; }

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
        public WhileStatement(Expression condition, InstructionBlock body)
        {
            this.Condition = condition;
            this.Body = body;
        }

        public Expression Condition { get; }

        public InstructionBlock Body { get; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>() { this.Condition, this.Body };
        }
    }

    public class IfStatement : Expression
    {
        public IfStatement(Expression condition, InstructionBlock thenBody, InstructionBlock elseBody)
        {
            this.Condition = condition;
            this.ThenBody = thenBody;
            this.ElseBody = elseBody;
        }

        public Expression Condition { get; }

        public InstructionBlock ThenBody { get; }

        public InstructionBlock ElseBody { get; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> { this.Condition, this.ThenBody, this.ElseBody };
        }
    }

    public class FunctionCall : Expression
    {
        public FunctionCall(string identifier, IList<Expression> arguments)
        {
            this.Identifier = identifier;
            this.Arguments = arguments;
        }

        public string Identifier { get; }

        public IList<Expression> Arguments { get; set; }

        public FunctionDeclaration Declaration { get; set; }

        public List<FunctionDeclaration> DeclarationCandidates { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Arguments);
        }
    }

    public class ReturnStatement : Expression
    {
        public ReturnStatement(Expression value)
        {
            this.Value = value;
        }

        public Expression Value { get; }

        public override IEnumerable<Node> Children()
        {
            if (this.Value == null)
                return new List<Node>();
            else
                return new List<Node>() { this.Value };
        }
    }

    public class BreakStatement : Expression
    {
        public WhileStatement EnclosingLoop { get; set; }
    }

    public class ContinueStatement : Expression
    {
        public WhileStatement EnclosingLoop { get; set; }
    }

    public class Variable : Expression
    {
        public Variable(string identifier)
        {
            this.Identifier = identifier;
        }

        public string Identifier { get; }

        public VariableDeclaration Declaration { get; set; }
    }

    public class BoolLiteral : Expression
    {
        public BoolLiteral(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }
    }

    public class IntegerLiteral : Expression
    {
        public IntegerLiteral(long value)
        {
            this.Value = value;
        }

        public long Value { get; }
    }

    public class UnitLiteral : Expression
    {
        public UnitLiteral()
        {
        }
    }

    public class Assignment : Expression
    {
        public Assignment(Variable lhs, Expression value)
        {
            this.Lhs = lhs;
            this.Value = value;
        }

        public Variable Lhs { get; }

        public Expression Value { get; }

        public override IEnumerable<Node> Children()
        {
            var result = new List<Node> { this.Lhs };
            if (this.Value != null)
                result.Add(this.Value);
            return result;
        }
    }

    public class CompoundAssignment : Expression
    {
        public CompoundAssignment(Variable lhs, ArithmeticOperationType operation, Expression value)
        {
            this.Lhs = lhs;
            this.Operation = operation;
            this.Value = value;
        }

        public Variable Lhs { get; }

        public ArithmeticOperationType Operation { get; }

        public Expression Value { get; }

        public override IEnumerable<Node> Children()
        {
            List<Node> result = new List<Node>() { this.Lhs };
            if (this.Value != null)
                result.Add(this.Value);
            return result;
        }
    }

    public abstract class BinaryOperation : Expression
    {
        public Expression LeftValue { get; set; }

        public Expression RightValue { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> { this.LeftValue, this.RightValue };
        }
    }

    public class ArithmeticOperation : BinaryOperation
    {
        public ArithmeticOperation(ArithmeticOperationType operationType, Expression leftValue, Expression rightValue)
        {
            this.OperationType = operationType;
            this.LeftValue = leftValue;
            this.RightValue = rightValue;
        }

        public ArithmeticOperationType OperationType { get; set; }
    }

    public class Comparison : BinaryOperation
    {
        public Comparison(ComparisonType operationType, Expression leftValue, Expression rightValue)
        {
            this.OperationType = operationType;
            this.LeftValue = leftValue;
            this.RightValue = rightValue;
        }

        public ComparisonType OperationType { get; set; }
    }

    public class LogicalBinaryOperation : BinaryOperation
    {
        public LogicalBinaryOperation(
            LogicalBinaryOperationType binaryOperationType,
            Expression leftValue,
            Expression rightValue)
        {
            this.BinaryOperationType = binaryOperationType;
            this.LeftValue = leftValue;
            this.RightValue = rightValue;
        }

        public LogicalBinaryOperationType BinaryOperationType { get; set; }
    }

    public class UnaryOperation : Expression
    {
        public UnaryOperation(UnaryOperationType unaryOperationType, Expression value)
        {
            this.UnaryOperationType = unaryOperationType;
            this.Value = value;
        }

        public UnaryOperationType UnaryOperationType { get; }

        public Expression Value { get; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> { this.Value };
        }
    }
}