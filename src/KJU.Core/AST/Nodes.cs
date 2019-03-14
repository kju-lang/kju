#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace KJU.Core.AST
{
    using System.Collections.Generic;

    public enum DataType
    {
        Unit, Bool, Int
    }

    public enum ArithmeticOperationType
    {
        Addition, Subtraction, Multiplication, Division, Remainder
    }

    public enum ComparisonType
    {
        Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual
    }

    public interface IExpression
    {
    }

    public class Program
    {
        public IEnumerable<FunctionDeclaration> Functions { get; set; }
    }

    public class FunctionDeclaration : IExpression
    {
        public string Identifier { get; set; }

        public DataType ReturnType { get; set; }

        public IEnumerable<FunctionParameter> Parameters { get; set; }

        public InstructionBlock Body { get; set; }
    }

    public class FunctionParameter
    {
        public DataType Type { get; set; }

        public string Identifier { get; set; }
    }

    public class InstructionBlock : IExpression
    {
        public IEnumerable<IExpression> Instructions { get; set; }
    }

    public class VariableDeclaration : IExpression
    {
        public DataType Type { get; set; }

        public string Identifier { get; set; }

        public IExpression Value { get; set; }
    }

    public class WhileStatement : IExpression
    {
        public IExpression Condition { get; set; }

        public InstructionBlock Body { get; set; }
    }

    public class IfStatement : IExpression
    {
        public IExpression Condition { get; set; }

        public InstructionBlock ThenBody { get; set; }

        public InstructionBlock ElseBody { get; set; }
    }

    public class FunctionCall : IExpression
    {
        public string Identifier { get; set; }

        public IEnumerable<IExpression> Arguments { get; set; }
    }

    public class ReturnStatement : IExpression
    {
        public IExpression Value { get; set; }
    }

    public class Variable : IExpression
    {
        public string Identifier { get; set; }
    }

    public class BoolLiteral : IExpression
    {
        public bool Value { get; set; }
    }

    public class IntegerLiteral : IExpression
    {
        public long Value { get; set; }
    }

    public class Assignment : IExpression
    {
        public string Identifier { get; set; }

        public IExpression Value { get; set; }
    }

    public class CompoundAssignment : IExpression
    {
        public string Identifier { get; set; }

        public ArithmeticOperationType Operation { get; set; }

        public IExpression Value { get; set; }
    }

    public class ArithmeticOperation : IExpression
    {
        public ArithmeticOperationType Type { get; set; }

        public IExpression LeftValue { get; set; }

        public IExpression RightValue { get; set; }
    }

    public class Comparison : IExpression
    {
        public ComparisonType Type { get; set; }

        public IExpression LeftValue { get; set; }

        public IExpression RightValue { get; set; }
    }
}
