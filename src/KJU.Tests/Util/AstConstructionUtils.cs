namespace KJU.Tests.Util
{
    using System.Collections.Generic;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.Input;
    using KJU.Core.Lexer;

    // Utils for manually constructing very simple AST's
    public static class AstConstructionUtils
    {
        public static FunctionDeclaration CreateFunction(string name, List<Expression> body)
        {
            return new FunctionDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                name,
                IntType.Instance,
                new List<VariableDeclaration>(),
                new InstructionBlock(new Range(new StringLocation(0), new StringLocation(1)), body),
                false);
        }

        public static VariableDeclaration CreateVariableDeclaration(string name)
        {
            return new VariableDeclaration(
                new Range(new StringLocation(0), new StringLocation(1)),
                IntType.Instance,
                name,
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 0));
        }

        public static Variable CreateVariable(VariableDeclaration declaration)
        {
            return new Variable(new Range(new StringLocation(0), new StringLocation(1)), declaration.Identifier)
                { Declaration = declaration };
        }

        public static Assignment CreateAssignment(VariableDeclaration declaration, Expression value)
        {
            return new Assignment(
                new Range(new StringLocation(0), new StringLocation(1)),
                CreateVariable(declaration),
                value);
        }

        public static CompoundAssignment CreateIncrement(VariableDeclaration declaration)
        {
            var variable = CreateVariable(declaration);
            return new CompoundAssignment(
                new Range(new StringLocation(0), new StringLocation(1)),
                variable,
                ArithmeticOperationType.Addition,
                new IntegerLiteral(new Range(new StringLocation(0), new StringLocation(1)), 1));
        }
    }
}