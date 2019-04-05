namespace KJU.Tests.Util
{
    using System.Collections.Generic;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;

    // Utils for manually constructing very simple AST's
    public class SimpleAstConstruction
    {
        public static FunctionDeclaration CreateFunction(string name, List<Expression> body)
        {
            return new FunctionDeclaration(name, IntType.Instance, new List<VariableDeclaration>(), new InstructionBlock(body));
        }

        public static VariableDeclaration CreateVariableDeclaration(string name)
        {
            return new VariableDeclaration(IntType.Instance, name, new IntegerLiteral(0));
        }

        public static Variable CreateVariable(VariableDeclaration declaration)
        {
            var variable = new Variable(declaration.Identifier);
            variable.Declaration = declaration;

            return variable;
        }

        public static Assignment CreateAssignment(VariableDeclaration declaration, Expression value)
        {
            return new Assignment(CreateVariable(declaration), value);
        }

        public static CompoundAssignment CreateIncrement(VariableDeclaration declaration)
        {
            return new CompoundAssignment(CreateVariable(declaration), ArithmeticOperationType.Addition, new IntegerLiteral(1));
        }
    }
}