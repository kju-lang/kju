namespace KJU.Core.Intermediate.TemporaryVariablesExtractor
{
    using System.Collections.Generic;
    using System.Linq;
    using AST;

    internal class TemporaryVariablesExtractor
    {
        private readonly IReadOnlyDictionary<AST.Node, IReadOnlyCollection<VariableDeclaration>>
            variableModificationGraph;

        private readonly IReadOnlyDictionary<AST.Node, IReadOnlyCollection<VariableDeclaration>>
            variableAccessGraph;

        public TemporaryVariablesExtractor(
            IReadOnlyDictionary<AST.Node, IReadOnlyCollection<VariableDeclaration>> variableModificationGraph,
            IReadOnlyDictionary<AST.Node, IReadOnlyCollection<VariableDeclaration>> variableAccessGraph)
        {
            this.variableModificationGraph = variableModificationGraph;
            this.variableAccessGraph = variableAccessGraph;
        }

        public List<Expression> ExtractTemporaryVariables(Expression node)
        {
            switch (node)
            {
                case FunctionDeclaration _:
                    return new List<Expression>();

                case InstructionBlock instructionBlock:

                    instructionBlock.Instructions = instructionBlock
                        .Instructions
                        .SelectMany(instruction => this.ExtractTemporaryVariables(instruction).Append(instruction))
                        .ToList();
                    return new List<Expression>();

                case VariableDeclaration variable:
                {
                    var value = variable.Value;
                    return value == null ? new List<Expression>() : this.ExtractTemporaryVariables(variable.Value);
                }

                case WhileStatement whileNode:
                    this.ExtractTemporaryVariables(whileNode.Body);
                    return this.ExtractTemporaryVariables(whileNode.Condition);

                case IfStatement ifNode:
                    this.ExtractTemporaryVariables(ifNode.ElseBody);
                    this.ExtractTemporaryVariables(ifNode.ThenBody);
                    return this.ExtractTemporaryVariables(ifNode.Condition);

                case AST.FunctionCall funCall:
                    funCall.Arguments = funCall.Arguments.Select((argument, i) =>
                    {
                        if (argument is AST.Variable variableArgument)
                        {
                            var modifiedByAnotherArgument = funCall
                                .Arguments
                                .Skip(i + 1)
                                .Any(followingArgument => this.variableModificationGraph[followingArgument]
                                    .Contains(variableArgument.Declaration));
                            if (modifiedByAnotherArgument)
                            {
                                var tmpDeclaration = new VariableDeclaration(argument.Type, "tmp", argument);
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

                case ReturnStatement returnNode:
                {
                    var value = returnNode.Value;
                    return value == null ? new List<Expression>() : this.ExtractTemporaryVariables(value);
                }

                case AST.Variable _:
                    return new List<Expression>();

                case BoolLiteral _:
                    return new List<Expression>();

                case IntegerLiteral _:
                    return new List<Expression>();

                case UnitLiteral _:
                    return new List<Expression>();
                case BinaryOperation operationNode:

                    operationNode.LeftValue = this.ReplaceWithBlock(operationNode.LeftValue);

                    var result = new List<Expression>();
                    var modifiedVariables = this.variableModificationGraph[operationNode.LeftValue];
                    var usedVariables = this.variableAccessGraph[operationNode.RightValue];
                    if (modifiedVariables.Any(x => usedVariables.Contains(x)))
                    {
                        var tmpDecl = new VariableDeclaration(
                            operationNode.LeftValue.Type, "tmp", operationNode.LeftValue);
                        var tmpVar = new AST.Variable("tmp") { Declaration = tmpDecl };

                        result.Add(tmpDecl);
                        operationNode.LeftValue = tmpVar;
                    }

                    operationNode.RightValue = this.ReplaceWithBlock(operationNode.RightValue);

                    return result;

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