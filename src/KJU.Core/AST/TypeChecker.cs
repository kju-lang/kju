namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Diagnostics;
    using KJU.Core.AST.BuiltinTypes;
    using KJU.Core.Lexer;

    public class TypeChecker : ITypeChecker
    {
        public const string IncorrectTypeDiagnostic = "Incorect type";
        public const string IncorrectNumberOfArgumentsDiagnostic = "Incorrect number of arguments";

        private IDiagnostics Diagnostics { get; set; }

        public void LinkTypes(Node root, IDiagnostics diagnostics)
        {
            this.Diagnostics = diagnostics;
            this.Dfs(root);
        }

        private void AddDiagnostic(DiagnosticStatus status, string type, string message, IReadOnlyList<Range> ranges)
        {
            this.Diagnostics.Add(new Diagnostic(status, type, message, ranges));
            return;
        }

        private void CheckReturnTypesMatch(Node node, DataType expectedType)
        {
            switch (node)
            {
                case ReturnStatement returnNode:
                    var type = returnNode.Type;
                    if (!expectedType.Equals(type))
                    {
                        this.AddDiagnostic(
                            DiagnosticStatus.Error,
                            IncorrectTypeDiagnostic,
                            $"Incorrect return type: expected {expectedType}, got {type}",
                            new List<Range>() { node.InputRange });
                    }

                    return;

                case FunctionDeclaration fun:
                    return;
            }

            foreach (Node child in node.Children())
            {
                this.CheckReturnTypesMatch(child, expectedType);
            }
        }

        private void CheckFunctionArgumentsTypes(FunctionCall funCall)
        {
            var arguments = funCall.Arguments;
            var parameters = funCall.Declaration.Parameters;

            if (arguments.Count != parameters.Count)
            {
                this.AddDiagnostic(
                    DiagnosticStatus.Error,
                    IncorrectNumberOfArgumentsDiagnostic,
                    $"Incorrect number of function arguments: expected {arguments.Count}, got {parameters.Count}",
                    new List<Range>() { funCall.InputRange });
            }

            var pairs = arguments.Zip(parameters, (argument, parameter) => new { Argument = argument, Parameter = parameter });
            foreach (var element in pairs)
            {
                if (!element.Argument.Type.Equals(element.Parameter.VariableType))
                {
                    this.AddDiagnostic(
                        DiagnosticStatus.Error,
                        IncorrectTypeDiagnostic,
                        $"Incorrect function argument type: expected {element.Parameter.Type}, got {element.Argument.Type}",
                        new List<Range>() { funCall.InputRange });
                }
            }
        }

        private void Dfs(Node node)
        {
            foreach (Node child in node.Children())
            {
                this.Dfs(child);
            }

            switch (node)
            {
                case Program p:
                    break;

                case FunctionDeclaration fun:
                    if (fun.ReturnType == null)
                        throw new TypeCheckerException("Function return type is null");
                    fun.Type = UnitType.Instance;

                    this.CheckReturnTypesMatch(fun.Body, fun.ReturnType);
                    break;

                case InstructionBlock instruction:
                    instruction.Type = UnitType.Instance;
                    break;

                case VariableDeclaration variable:
                    if (variable.VariableType == null)
                        throw new TypeCheckerException("Variable type is null");
                    variable.Type = UnitType.Instance;
                    break;

                case WhileStatement whileNode:
                    whileNode.Type = UnitType.Instance;
                    break;

                case IfStatement ifNode:
                    ifNode.Type = UnitType.Instance;
                    break;

                case FunctionCall funCall:
                    funCall.Type = funCall.Declaration.ReturnType;

                    this.CheckFunctionArgumentsTypes(funCall);
                    break;

                case ReturnStatement returnNode:
                    returnNode.Type = returnNode.Value == null ? UnitType.Instance : returnNode.Value.Type;
                    break;

                case Variable variable:
                    variable.Type = variable.Declaration.VariableType;
                    break;

                case BoolLiteral boolNode:
                    boolNode.Type = BoolType.Instance;
                    break;

                case IntegerLiteral integerNode:
                    integerNode.Type = IntType.Instance;
                    break;

                case Assignment assignmentNode:
                    if (assignmentNode.Value == null)
                        throw new TypeCheckerException("Assignment value is null");

                    if (!assignmentNode.Lhs.Type.Equals(assignmentNode.Value.Type))
                    {
                        this.AddDiagnostic(
                            DiagnosticStatus.Error,
                            IncorrectTypeDiagnostic,
                            $"Incorrect assignment value type: expected {assignmentNode.Lhs.Type}, got {assignmentNode.Value.Type}",
                            new List<Range>() { assignmentNode.Lhs.InputRange, assignmentNode.Value.InputRange });
                    }

                    assignmentNode.Type = assignmentNode.Lhs.Type;
                    break;

                case CompoundAssignment compoundNode:
                    if (compoundNode.Value == null)
                        throw new TypeCheckerException("Compound assignment value is null");

                    if (!compoundNode.Lhs.Type.Equals(IntType.Instance))
                    {
                        this.AddDiagnostic(
                            DiagnosticStatus.Error,
                            IncorrectTypeDiagnostic,
                            $"Incorrect left hand size type: expected {IntType.Instance}, got {compoundNode.Lhs.Type}",
                            new List<Range>() { compoundNode.Lhs.InputRange });
                    }

                    if (!compoundNode.Value.Type.Equals(IntType.Instance))
                    {
                        this.AddDiagnostic(
                            DiagnosticStatus.Error,
                            IncorrectTypeDiagnostic,
                            $"Incorrect right hand size type: expected {IntType.Instance}, got {compoundNode.Value.Type}",
                            new List<Range>() { compoundNode.Value.InputRange });
                    }

                    compoundNode.Type = compoundNode.Lhs.Type;
                    break;

                case ArithmeticOperation operationNode:
                    foreach (var operand in new List<Expression>() { operationNode.LeftValue, operationNode.RightValue })
                    {
                        var type = operand.Type;
                        if (type == null)
                            throw new TypeCheckerException("Operand type is null");

                        if (!type.Equals(IntType.Instance))
                        {
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectTypeDiagnostic,
                                $"Incorrect operand type: expected {IntType.Instance}, got {type}",
                                new List<Range>() { operand.InputRange });
                        }
                    }

                    operationNode.Type = IntType.Instance;
                    break;

                case Comparison comparisonNode:
                    if (!comparisonNode.LeftValue.Type.Equals(comparisonNode.RightValue.Type))
                    {
                        this.AddDiagnostic(
                            DiagnosticStatus.Error,
                            IncorrectTypeDiagnostic,
                            $"Type mismatch: left hand size {comparisonNode.LeftValue.Type}, right hand side {comparisonNode.RightValue.Type}",
                            new List<Range>() { comparisonNode.LeftValue.InputRange, comparisonNode.RightValue.InputRange });
                    }

                    comparisonNode.Type = BoolType.Instance;
                    break;

                case Expression e:
                    throw new TypeCheckerException("Unrecognized node type");
            }
        }
    }
}