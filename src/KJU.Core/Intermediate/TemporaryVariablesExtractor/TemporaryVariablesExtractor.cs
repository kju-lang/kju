namespace KJU.Core.Intermediate.TemporaryVariablesExtractor
{
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using KJU.Core.AST.VariableAccessGraph;

    internal class TemporaryVariablesExtractor
    {
        private readonly VariableAccess variableAccess;

        private readonly Function.Function function;

        public TemporaryVariablesExtractor(
            VariableAccess variableAccess,
            Function.Function function)
        {
            this.variableAccess = variableAccess;
            this.function = function;
        }

        public List<Expression> ExtractTemporaryVariables(Expression node)
        {
            switch (node)
            {
                case FunctionDeclaration _:
                    return new List<Expression>();

                case InstructionBlock instructionBlock:
                    return this.ExtractFromInstructionBlock(instructionBlock);

                case VariableDeclaration variable:
                    return this.ExtractFromVariable(variable);

                case WhileStatement whileNode:
                    return this.ExtractFromWhile(whileNode);

                case IfStatement ifNode:
                    return this.ExtractFromIf(ifNode);

                case AST.FunctionCall funCall:
                    return this.ExtractFromFunctionCall(funCall);

                case ReturnStatement returnStatement:
                    return this.ExtractFromReturnStatement(returnStatement);

                case AST.Variable _:
                    return new List<Expression>();

                case BoolLiteral _:
                    return new List<Expression>();

                case IntegerLiteral _:
                    return new List<Expression>();

                case UnitLiteral _:
                    return new List<Expression>();
                case BinaryOperation operationNode:
                    return this.ExtractFromOperationNode(operationNode);

                case Assignment assignmentNode:
                    return this.ExtractTemporaryVariables(assignmentNode.Value);

                case CompoundAssignment compoundNode:
                    return this.ExtractTemporaryVariables(compoundNode.Value);

                case AST.UnaryOperation unaryOperation:
                    return this.ExtractTemporaryVariables(unaryOperation.Value);
                case BreakStatement _:
                    return new List<Expression>();
                case null:
                    throw new TemporaryVariablesExtractorException(
                        $"Null AST node. Should this ever happen?");
                default:
                    throw new TemporaryVariablesExtractorException(
                        $"Unexpected AST node type: {node.GetType()}. This should never happen.");
            }
        }

        private List<Expression> ExtractFromOperationNode(BinaryOperation operationNode)
        {
            operationNode.LeftValue = this.ReplaceWithBlock(operationNode.LeftValue);

            var modifiedVariables = this.variableAccess.Modifies[operationNode.LeftValue];
            var usedVariables = this.variableAccess.Accesses[operationNode.RightValue];
            var result = new List<Expression>();
            if (modifiedVariables.Any(x => usedVariables.Contains(x)))
            {
                var tmpDecl = new VariableDeclaration(
                    operationNode.LeftValue.Type, "tmp", operationNode.LeftValue)
                {
                    IntermediateVariable = new Intermediate.Variable(
                        this.function,
                        new VirtualRegister())
                };
                var tmpVar = new AST.Variable("tmp") { Declaration = tmpDecl };

                result.Add(tmpDecl);
                operationNode.LeftValue = tmpVar;
            }

            operationNode.RightValue = this.ReplaceWithBlock(operationNode.RightValue);

            return result;
        }

        private List<Expression> ExtractFromReturnStatement(ReturnStatement returnNode)
        {
            var value = returnNode.Value;
            return value == null ? new List<Expression>() : this.ExtractTemporaryVariables(value);
        }

        private List<Expression> ExtractFromFunctionCall(FunctionCall funCall)
        {
            funCall.Arguments = funCall.Arguments.Select((argument, i) =>
            {
                if (argument is AST.Variable variableArgument)
                {
                    var isModifiedByAnotherArgument = funCall
                        .Arguments
                        .Skip(i + 1)
                        .Any(followingArgument => this.variableAccess.Modifies[followingArgument]
                            .Contains(variableArgument.Declaration));
                    if (isModifiedByAnotherArgument)
                    {
                        var tmpDeclaration = new VariableDeclaration(argument.Type, "tmp", argument)
                        {
                            IntermediateVariable = new Intermediate.Variable(this.function, new VirtualRegister())
                        };
                        var tmpVariable = new AST.Variable("tmp")
                            { Declaration = tmpDeclaration, Type = argument.Type };
                        return (Expression)new BlockWithResult(
                            new InstructionBlock(new List<Expression> { tmpDeclaration }),
                            tmpVariable)
                        {
                            Type = tmpVariable.Type
                        };
                    }
                }

                var instructions = this.ExtractTemporaryVariables(argument);
                return new BlockWithResult(new InstructionBlock(instructions), argument)
                {
                    Type = argument.Type
                };
            }).ToList();
            return new List<Expression>();
        }

        private List<Expression> ExtractFromIf(IfStatement ifNode)
        {
            this.ExtractTemporaryVariables(ifNode.ElseBody);
            this.ExtractTemporaryVariables(ifNode.ThenBody);
            return this.ExtractTemporaryVariables(ifNode.Condition);
        }

        private List<Expression> ExtractFromWhile(WhileStatement whileNode)
        {
            this.ExtractTemporaryVariables(whileNode.Body);
            return this.ExtractTemporaryVariables(whileNode.Condition);
        }

        private List<Expression> ExtractFromVariable(VariableDeclaration variable)
        {
            var value = variable.Value;
            return value == null ? new List<Expression>() : this.ExtractTemporaryVariables(variable.Value);
        }

        private List<Expression> ExtractFromInstructionBlock(InstructionBlock instructionBlock)
        {
            instructionBlock.Instructions = instructionBlock
                .Instructions
                .SelectMany(instruction => this.ExtractTemporaryVariables(instruction).Append(instruction))
                .ToList();
            return new List<Expression>();
        }

        private Expression ReplaceWithBlock(Expression expression)
        {
            var tmpResult = this.ExtractTemporaryVariables(expression);
            if (tmpResult.Count == 0)
            {
                return expression;
            }

            return new BlockWithResult(new InstructionBlock(tmpResult), expression)
            {
                Type = expression.Type
            };
        }
    }
}