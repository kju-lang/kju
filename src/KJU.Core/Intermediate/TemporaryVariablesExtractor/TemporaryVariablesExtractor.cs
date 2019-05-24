namespace KJU.Core.Intermediate.TemporaryVariablesExtractor
{
    using System.Collections.Generic;
    using System.Linq;
    using AST;
    using AST.Nodes;
    using AST.VariableAccessGraph;

    public class TemporaryVariablesExtractor
    {
        public IEnumerable<Expression> ExtractTemporaryVariables(VariableAccess variableAccess, Expression node)
        {
            return new ExtractorProcess(variableAccess).ExtractTemporaryVariables(node);
        }

        private class ExtractorProcess
        {
            private readonly VariableAccess variableAccess;

            public ExtractorProcess(VariableAccess variableAccess)
            {
                this.variableAccess = variableAccess;
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

                    case ArrayAlloc arrayAlloc:
                        return this.ExtractTemporaryVariables(arrayAlloc.Size);

                    case ArrayAccess arrayAccess:
                        return this.ExtractFromArrayAccess(arrayAccess);

                    case IComplexAssignment arrayAssignment:
                        return this.ExtractFromArrayAssignment(arrayAssignment);

                    case WhileStatement whileNode:
                        return this.ExtractFromWhile(whileNode);

                    case IfStatement ifNode:
                        return this.ExtractFromIf(ifNode);

                    case StructDeclaration _:
                    case StructAlloc _:
                        return new List<Expression>();

                    case FieldAccess fieldAccess:
                        return this.ExtractFromFieldAccess(fieldAccess);

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
                    case NullLiteral _:
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

            private List<Expression> ExtractFromFieldAccess(FieldAccess fieldAccess)
            {
                this.ExtractTemporaryVariables(fieldAccess.Lhs);
                return new List<Expression>();
            }

            private List<Expression> ExtractFromOperationNode(BinaryOperation operationNode)
            {
                // In (A op B)
                // We'd like to compute A before B if both A and B use variable x and at least one of them modifies it

                var modifiedVariablesLeft = this.variableAccess.Modifies[operationNode.LeftValue];
                var modifiedVariablesRight = this.variableAccess.Modifies[operationNode.RightValue];
                var usedVariablesLeft = this.variableAccess.Accesses[operationNode.LeftValue];
                var usedVariablesRight = this.variableAccess.Accesses[operationNode.RightValue];

                operationNode.LeftValue = this.ReplaceWithBlock(operationNode.LeftValue);

                var result = new List<Expression>();
                if (modifiedVariablesLeft.Any(x => usedVariablesRight.Contains(x))
                    || modifiedVariablesRight.Any(x => usedVariablesLeft.Contains(x)))
                {
                    var tmpDecl = new VariableDeclaration(
                        operationNode.LeftValue.InputRange,
                        operationNode.LeftValue.Type,
                        "tmp",
                        operationNode.LeftValue)
                    {
                        IntermediateVariable = new VirtualRegister()
                    };
                    var tmpVar = new AST.Variable(tmpDecl.InputRange, "tmp") { Declaration = tmpDecl };

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
                            var tmpDeclaration =
                                new VariableDeclaration(argument.InputRange, argument.Type, "tmp", argument)
                                {
                                    IntermediateVariable = new VirtualRegister()
                                };
                            var tmpVariable = new AST.Variable(tmpDeclaration.InputRange, "tmp")
                                { Declaration = tmpDeclaration, Type = argument.Type };
                            var instructionBlock = new InstructionBlock(
                                tmpDeclaration.InputRange,
                                new List<Expression> { tmpDeclaration });
                            return (Expression)new BlockWithResult(
                                tmpDeclaration.InputRange,
                                instructionBlock,
                                tmpVariable)
                            {
                                Type = tmpVariable.Type
                            };
                        }
                    }

                    var instructions = this.ExtractTemporaryVariables(argument);
                    return new BlockWithResult(
                        funCall.InputRange,
                        new InstructionBlock(funCall.InputRange, instructions),
                        argument)
                    {
                        Type = argument.Type
                    };
                }).ToList();
                return new List<Expression>();
            }

            private List<Expression> ExtractFromArrayAccess(ArrayAccess access)
            {
                var binaryOperation = new BinaryOperation(access.InputRange, access.Lhs, access.Index);
                var result = this.ExtractFromOperationNode(binaryOperation);
                (access.Lhs, access.Index) = (binaryOperation.LeftValue, binaryOperation.RightValue);
                return result;
            }

            private List<Expression> ExtractFromArrayAssignment(IComplexAssignment complexAssignment)
            {
                var binaryOperation = new BinaryOperation(
                    complexAssignment.InputRange,
                    complexAssignment.Lhs,
                    complexAssignment.Value);
                var result = this.ExtractFromOperationNode(binaryOperation);
                (complexAssignment.Lhs, complexAssignment.Value) = (binaryOperation.LeftValue, binaryOperation.RightValue);
                return result;
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

                return new BlockWithResult(
                    expression.InputRange,
                    new InstructionBlock(expression.InputRange, tmpResult),
                    expression)
                {
                    Type = expression.Type
                };
            }
        }
    }
}
