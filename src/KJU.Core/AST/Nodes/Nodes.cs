#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AST.Nodes;
    using KJU.Core.Intermediate.Function;

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

        public override string ToString()
        {
            return $"Program";
        }
    }

    public class FunctionDeclaration : Expression
    {
        public FunctionDeclaration(
            string identifier,
            DataType returnType,
            IReadOnlyList<VariableDeclaration> parameters,
            InstructionBlock body,
            bool isForeign)
        {
            this.Identifier = identifier;
            this.ReturnType = returnType;
            this.Parameters = parameters;
            this.Body = body;
            this.IsForeign = isForeign;
        }

        public string Identifier { get; }

        public DataType ReturnType { get; set; }

        public IReadOnlyList<VariableDeclaration> Parameters { get; }

        public InstructionBlock Body { get; set; }

        public bool IsForeign { get; }

        public Function IntermediateFunction { get; set; }

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

        public bool IsEntryPoint()
        {
            return this.Identifier == "kju" && this.Parameters.Count == 0;
        }

        public override IEnumerable<Node> Children()
        {
            return this.Body != null ? new List<Node>(this.Parameters) { this.Body } : new List<Node>(this.Parameters);
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

        public override string ToString()
        {
            return $"Instruction Block";
        }
    }

    public class VariableDeclaration : Expression
    {
        private Intermediate.Variable intermediateVariable;

        public VariableDeclaration(DataType variableType, string identifier, Expression value)
        {
            this.VariableType = variableType;
            this.Identifier = identifier;
            this.Value = value;
        }

        public DataType VariableType { get; set; }

        public string Identifier { get; }

        public Expression Value { get; }

        public Intermediate.Variable IntermediateVariable
        {
            get => this.intermediateVariable;
            set => this.intermediateVariable = value ?? throw new Exception("Intermediate variable is null.");
        }

        public override string Representation()
        {
            return
                $"VariableDeclaration{{VariableType: {this.VariableType}, Identifier: {this.Identifier}, Value: {this.Value}, IntermediateVariable: {this.intermediateVariable?.ToString() ?? "null!!!!"}}}";
        }

        public override IEnumerable<Node> Children()
        {
            return this.Value == null ? new List<Node>() : new List<Node>() { this.Value };
        }

        public override string ToString()
        {
            return $"var {this.Identifier} : {this.VariableType}";
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

        public override string ToString()
        {
            return $"While";
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

        public override string ToString()
        {
            return $"If";
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

        public override string ToString()
        {
            return $"{this.Identifier}()";
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

        public override string ToString()
        {
            return $"Return";
        }
    }

    public class BreakStatement : Expression
    {
        public WhileStatement EnclosingLoop { get; set; }

        public override string ToString()
        {
            return $"Break";
        }
    }

    public class ContinueStatement : Expression
    {
        public WhileStatement EnclosingLoop { get; set; }

        public override string ToString()
        {
            return $"Continue";
        }
    }

    public class Variable : Expression
    {
        public Variable(string identifier)
        {
            this.Identifier = identifier;
        }

        public string Identifier { get; }

        public VariableDeclaration Declaration { get; set; }

        public override string ToString()
        {
            return $"{this.Identifier}";
        }
    }

    public class BoolLiteral : Expression
    {
        public BoolLiteral(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public override string ToString()
        {
            return $"{this.Value}";
        }
    }

    public class IntegerLiteral : Expression
    {
        public IntegerLiteral(long value)
        {
            this.Value = value;
        }

        public long Value { get; }

        public override string ToString()
        {
            return $"{this.Value}";
        }
    }

    public class UnitLiteral : Expression
    {
        public UnitLiteral()
        {
        }

        public override string ToString()
        {
            return $"Unit";
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

        public override string ToString()
        {
            return $"Assignment";
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

        public override string ToString()
        {
            return $"{this.Operation} Assignment";
        }
    }

    public class ArrayAssignment : Expression, IArrayAssignment
    {
        public ArrayAssignment(Expression lhs, Expression value)
        {
            this.Lhs = lhs;
            this.Value = value;
        }

        public Expression Lhs { get; set; }

        public Expression Value { get; set; }

        public override IEnumerable<Node> Children()
        {
            var result = new List<Node> { this.Lhs };
            if (this.Value != null)
                result.Add(this.Value);
            return result;
        }

        public override string ToString()
        {
            return $"ArrayAssignment";
        }
    }

    public class ArrayCompoundAssignment : Expression, IArrayAssignment
    {
        public ArrayCompoundAssignment(Expression lhs, ArithmeticOperationType operation, Expression value)
        {
            this.Lhs = lhs;
            this.Operation = operation;
            this.Value = value;
        }

        public Expression Lhs { get; set; }

        public ArithmeticOperationType Operation { get; }

        public Expression Value { get; set; }

        public override IEnumerable<Node> Children()
        {
            List<Node> result = new List<Node>() { this.Lhs };
            if (this.Value != null)
                result.Add(this.Value);
            return result;
        }

        public override string ToString()
        {
            return $"{this.Operation} ArrayAssignment";
        }
    }

    public class BinaryOperation : Expression
    {
        public BinaryOperation(Expression lhs, Expression rhs)
        {
            this.LeftValue = lhs;
            this.RightValue = rhs;
        }

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
            : base(leftValue, rightValue)
        {
            this.OperationType = operationType;
        }

        public ArithmeticOperationType OperationType { get; set; }

        public override string ToString()
        {
            return $"{this.OperationType}";
        }
    }

    public class Comparison : BinaryOperation
    {
        public Comparison(ComparisonType operationType, Expression leftValue, Expression rightValue)
            : base(leftValue, rightValue)
        {
            this.OperationType = operationType;
        }

        public ComparisonType OperationType { get; set; }

        public override string ToString()
        {
            return $"{this.OperationType}";
        }
    }

    public class LogicalBinaryOperation : BinaryOperation
    {
        public LogicalBinaryOperation(
            LogicalBinaryOperationType binaryOperationType,
            Expression leftValue,
            Expression rightValue)
            : base(leftValue, rightValue)
        {
            this.BinaryOperationType = binaryOperationType;
        }

        public LogicalBinaryOperationType BinaryOperationType { get; set; }

        public override string ToString()
        {
            return $"{this.BinaryOperationType}";
        }
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

        public override string ToString()
        {
            return $"{this.UnaryOperationType}";
        }
    }

    public class ArrayAccess : Expression
    {
        public ArrayAccess(Expression lhs, Expression index)
        {
            this.Lhs = lhs;
            this.Index = index;
        }

        public Expression Lhs { get; set; }

        public Expression Index { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> { this.Lhs, this.Index };
        }

        public override string ToString()
        {
            return $"ArrayAccess {this.Lhs} {this.Index}";
        }
    }

    public class ArrayAlloc : Expression
    {
        public ArrayAlloc(DataType elementType, Expression size)
        {
            this.ElementType = elementType;
            this.Size = size;
        }

        public DataType ElementType { get; set; }

        public Expression Size { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> { this.Size };
        }

        public override string ToString()
        {
            return $"ArrayAlloc {this.ElementType} {this.Size}";
        }
    }
}