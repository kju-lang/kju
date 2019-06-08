#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using Intermediate.Function;
    using KJU.Core.AST.Types;
    using KJU.Core.Lexer;
    using Nodes;

    public class Expression : Node
    {
        private DataType type;

        public Expression(Range inputRange)
: base(inputRange)
        {
        }

        public DataType Type
        {
            get
            {
                if (this.type == null)
                    this.type = new TypeVariable { InputRange = this.InputRange };
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }
    }

    public class Program : Node
    {
        public Program(
            Range inputRange, IReadOnlyList<StructDeclaration> structs, IReadOnlyList<FunctionDeclaration> functions)
            : base(inputRange)
        {
            this.Structs = structs;
            this.Functions = functions;
        }

        public IReadOnlyList<StructDeclaration> Structs { get; }

        public IReadOnlyList<FunctionDeclaration> Functions { get; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>(this.Structs.Concat<Node>(this.Functions));
        }

        public override string ToString()
        {
            return $"Program";
        }
    }

    public class FunctionDeclaration : Expression
    {
        public FunctionDeclaration(
            Range inputRange,
            string identifier,
            DataType returnType,
            IReadOnlyList<VariableDeclaration> parameters,
            InstructionBlock body,
            bool isForeign)
            : base(inputRange)
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

        public Function Function { get; set; }

        public bool IsEntryPoint => this.Identifier == "kju" && this.Parameters.Count == 0;

        public static bool ParametersTypesEquals(FunctionDeclaration left, FunctionDeclaration right)
        {
            return ParametersTypesEquals(
                left.Parameters.Select(x => x.VariableType).ToList(),
                right.Parameters.Select(x => x.VariableType).ToList());
        }

        public static bool ParametersTypesEquals(IReadOnlyList<Expression> left, IReadOnlyList<DataType> right)
        {
            return ParametersTypesEquals(left.Select(x => x.Type).ToList(), right);
        }

        public static bool ParametersTypesEquals(FunctionDeclaration left, IReadOnlyList<DataType> right)
        {
            return ParametersTypesEquals(left.Parameters.Select(x => x.VariableType).ToList(), right);
        }

        public static bool ParametersTypesEquals(IReadOnlyList<DataType> left, IReadOnlyList<DataType> right)
        {
            return left.SequenceEqual(right);
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
        public InstructionBlock(Range inputRange, IReadOnlyList<Expression> instructions)
            : base(inputRange)
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
        public VariableDeclaration(Range inputRange, DataType variableType, string identifier, Expression value)
            : base(inputRange)
        {
            this.VariableType = variableType;
            this.Identifier = identifier;
            this.Value = value;
        }

        public DataType VariableType { get; set; }

        public string Identifier { get; }

        public Expression Value { get; }

        public ILocation IntermediateVariable { get; set; }

        public virtual string Representation()
        {
            return
                $"VariableDeclaration{{VariableType: {this.VariableType}, Identifier: {this.Identifier}, Value: {this.Value}, IntermediateVariable: {this.IntermediateVariable?.ToString() ?? "null!!!!"}}}";
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
        public WhileStatement(Range inputRange, Expression condition, InstructionBlock body)
            : base(inputRange)
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
        public IfStatement(Range inputRange, Expression condition, InstructionBlock thenBody, InstructionBlock elseBody)
            : base(inputRange)
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
        public FunctionCall(Range inputRange, string identifier, IList<Expression> arguments)
            : base(inputRange)
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
        public ReturnStatement(Range inputRange, Expression value)
            : base(inputRange)
        {
            this.Value = value;
        }

        public Expression Value { get; }

        public override IEnumerable<Node> Children()
        {
            return this.Value == null ? new List<Node>() : new List<Node>() { this.Value };
        }

        public override string ToString()
        {
            return $"Return";
        }
    }

    public class BreakStatement : Expression
    {
        public BreakStatement(Range inputRange)
            : base(inputRange)
        {
        }

        public WhileStatement EnclosingLoop { get; set; }

        public override string ToString()
        {
            return $"Break";
        }
    }

    public class ContinueStatement : Expression
    {
        public ContinueStatement(Range inputRange)
            : base(inputRange)
        {
        }

        public WhileStatement EnclosingLoop { get; set; }

        public override string ToString()
        {
            return $"Continue";
        }
    }

    public class Variable : Expression
    {
        public Variable(Range inputRange, string identifier)
            : base(inputRange)
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
        public BoolLiteral(Range inputRange, bool value)
            : base(inputRange)
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
        public IntegerLiteral(Range inputRange, long value)
            : base(inputRange)
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
        public UnitLiteral(Range inputRange)
            : base(inputRange)
        {
        }

        public override string ToString()
        {
            return $"Unit";
        }
    }

    public class NullLiteral : Expression
    {
        public NullLiteral(Range inputRange)
            : base(inputRange)
        {
        }

        public override string ToString()
        {
            return $"Null";
        }
    }

    public class Assignment : Expression
    {
        public Assignment(Range inputRange, Variable lhs, Expression value)
            : base(inputRange)
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
            {
                result.Add(this.Value);
            }

            return result;
        }

        public override string ToString()
        {
            return $"Assignment";
        }
    }

    public class CompoundAssignment : Expression
    {
        public CompoundAssignment(Range inputRange, Variable lhs, ArithmeticOperationType operation, Expression value)
            : base(inputRange)
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
            {
                result.Add(this.Value);
            }

            return result;
        }

        public override string ToString()
        {
            return $"{this.Operation} Assignment";
        }
    }

    public class ComplexAssignment : Expression, IComplexAssignment
    {
        public ComplexAssignment(Range inputRange, Expression lhs, Expression value)
            : base(inputRange)
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
            {
                result.Add(this.Value);
            }

            return result;
        }

        public override string ToString()
        {
            return $"ComplexAssignment";
        }
    }

    public class ComplexCompoundAssignment : Expression, IComplexAssignment
    {
        public ComplexCompoundAssignment(
            Range inputRange, Expression lhs, ArithmeticOperationType operation, Expression value)
            : base(inputRange)
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
            return $"{this.Operation} ComplexAssignment";
        }
    }

    public class BinaryOperation : Expression
    {
        public BinaryOperation(Range inputRange, Expression leftValue, Expression rightValue)
            : base(inputRange)
        {
            this.LeftValue = leftValue;
            this.RightValue = rightValue;
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
        public ArithmeticOperation(
            Range inputRange, Expression leftValue, Expression rightValue, ArithmeticOperationType operationType)
            : base(inputRange, leftValue, rightValue)
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
        public Comparison(Range inputRange, Expression leftValue, Expression rightValue, ComparisonType operationType)
            : base(inputRange, leftValue, rightValue)
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
            Range inputRange,
            Expression leftValue,
            Expression rightValue,
            LogicalBinaryOperationType binaryOperationType)
            : base(inputRange, leftValue, rightValue)
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
        public UnaryOperation(Range inputRange, UnaryOperationType unaryOperationType, Expression value)
            : base(inputRange)
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

    public class ArrayAccess : Expression, IContainerAccess
    {
        public ArrayAccess(Range inputRange, Expression lhs, Expression index)
            : base(inputRange)
        {
            this.Lhs = lhs;
            this.Index = index;
        }

        public Expression Lhs { get; set; }

        public Expression Index { get; set; }

        public Expression Offset { get => this.Index; }

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
        public ArrayAlloc(Range inputRange, DataType elementType, Expression size)
            : base(inputRange)
        {
            this.ElementType = elementType;
            this.Size = size;
        }

        public DataType ElementType { get; set; }

        public Expression Size { get; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> { this.Size };
        }

        public override string ToString()
        {
            return $"ArrayAlloc {this.ElementType} {this.Size}";
        }
    }

    public class FieldAccess : Expression, IContainerAccess
    {
        public FieldAccess(Range inputRange, Expression lhs, string field)
            : base(inputRange)
        {
            this.Lhs = lhs;
            this.Field = field;
        }

        public Expression Lhs { get; }

        public string Field { get; }

        // Warning: The AST builder holds a reference to the below list and modifies it. Do not copy it.
        public List<KeyValuePair<StructDeclaration, StructField>> StructCandidates { get; set; }

        public Expression Offset
        {
            get
            {
                switch (this.Lhs.Type)
                {
                    case AST.Types.StructType type:
                        var offset = type.Fields
                            .TakeWhile(x => x.Name != this.Field)
                            .Count();
                        return new AST.IntegerLiteral(this.InputRange, offset);
                    default:
                        throw new Exception($"Incorrect type of FieldAccess node: {this.Lhs.Type.ToString()}");
                }
            }
        }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> { this.Lhs };
        }

        public override string ToString()
        {
            return $"FieldAccess {this.Field}";
        }
    }

    public class StructDeclaration : Expression
    {
        public StructDeclaration(Range inputRange, string name, IReadOnlyList<StructField> fields)
            : base(inputRange)
        {
            this.Name = name;
            this.Fields = fields;
        }

        public string Name { get; }

        public StructType StructType { get; set; }

        public IReadOnlyList<StructField> Fields { get; }

        public override IEnumerable<Node> Children()
        {
            return this.Fields;
        }

        public override string ToString()
        {
            return $"StructDeclaration {this.Name}";
        }
    }

    public class StructField : Node
    {
        public StructField(Range inputRange, string name, DataType type)
            : base(inputRange)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; }

        public DataType Type { get; set; }

        public override string ToString()
        {
            return $"StructField {this.Name}: {this.Type.GetType()}";
        }
    }

    public class StructAlloc : Expression
    {
        public StructAlloc(Range inputRange, DataType allocType)
            : base(inputRange)
        {
            this.AllocType = allocType;
        }

        public DataType AllocType { get; set; }

        public StructDeclaration Declaration { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new List<Node>();
        }

        public override string ToString()
        {
            return $"StructAlloc {this.AllocType}";
        }
    }

    public class Application : Expression
    {
        public Application(Range inputRange, Expression function, IReadOnlyList<Expression> arguments)
            : base(inputRange)
        {
            this.Function = function;
            this.Arguments = arguments;
        }

        public Expression Function { get; }

        public IReadOnlyList<Expression> Arguments { get; set; }

        public override IEnumerable<Node> Children()
        {
            return new[] { this.Function }.Concat(this.Arguments);
        }

        public override string ToString()
        {
            return $"apply {this.Function}";
        }
    }

    public class UnApplication : Expression
    {
        public UnApplication(Range inputRange, string functionName)
            : base(inputRange)
        {
            this.FunctionName = functionName;
        }

        public string FunctionName { get; }

        public IReadOnlyCollection<FunctionDeclaration> Candidates { get; set; }

        public FunctionDeclaration Declaration { get; set; }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }

        public override string ToString()
        {
            return $"unapply {this.FunctionName}";
        }
    }
}
