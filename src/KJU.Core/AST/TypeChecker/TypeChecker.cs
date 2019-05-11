namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BuiltinTypes;
    using Diagnostics;
    using Lexer;
    using Types;

    public class TypeChecker : IPhase
    {
        public const string IncorrectReturnTypeDiagnostic = "TypeChecker.IncorrectReturnType";
        public const string IncorrectAssigmentTypeDiagnostic = "TypeChecker.IncorrectAssigmentType";
        public const string IncorrectIfPredicateTypeDiagnostic = "TypeChecker.IncorrectIfPredicateType";
        public const string IncorrectLeftSideTypeDiagnostic = "TypeChecker.IncorrectLeftSideType";
        public const string IncorrectRightSideTypeDiagnostic = "TypeChecker.IncorrectRightSideType";
        public const string IncorrectOperandTypeDiagnostic = "TypeChecker.IncorrectOperandType";
        public const string IncorrectComparisonTypeDiagnostic = "TypeChecker.IncorrectComparisonType";
        public const string IncorrectUnaryExpressionTypeDiagnostic = "TypeChecker.IncorrectUnaryExpressionType";
        public const string IncorrectArraySizeTypeDiagnostic = "TypeChecker.IncorrectArraySizeType";
        public const string IncorrectArrayIndexTypeDiagnostic = "TypeChecker.IncorrectArrayIndexType";
        public const string IncorrectArrayTypeDiagnostic = "TypeChecker.IncorrectArrayType";
        public const string IncorrectArrayAccessUseDiagnostic = "TypeChecker.IncorrectArrayAccessUse";
        public const string AssignedValueHasNoTypeDiagnostic = "TypeChecker.AssignedValueHasNoType";
        public const string FunctionOverloadNotFoundDiagnostic = "TypeChecker.FunctionOverloadNotFound";

        private static readonly IDictionary<UnaryOperationType, DataType> UnaryOperationToType =
            new Dictionary<UnaryOperationType, DataType>
            {
                [UnaryOperationType.Not] = BoolType.Instance,
                [UnaryOperationType.Plus] = IntType.Instance,
                [UnaryOperationType.Minus] = IntType.Instance
            };

        public void Run(Node root, IDiagnostics diagnostics)
        {
            new TypeCheckerProcess(diagnostics).LinkTypes(root);
        }

        private class TypeCheckerProcess
        {
            private readonly IDiagnostics diagnostics;
            private readonly List<Exception> exceptions = new List<Exception>();

            public TypeCheckerProcess(IDiagnostics diagnostics)
            {
                this.diagnostics = diagnostics;
            }

            public void LinkTypes(Node root)
            {
                this.Dfs(root);
                if (this.exceptions.Any())
                {
                    throw new TypeCheckerException("Type checking failed.", this.exceptions);
                }
            }

            private void AddDiagnostic(
                DiagnosticStatus status,
                string type,
                string message,
                IReadOnlyList<Range> ranges)
            {
                this.diagnostics.Add(new Diagnostic(status, type, message, ranges));
            }

            private void CheckReturnTypesMatch(Node node, DataType expectedType)
            {
                switch (node)
                {
                    case ReturnStatement returnNode:
                        var type = returnNode.Type;
                        if (!expectedType.Equals(type))
                        {
                            var message = $"Incorrect return type. Expected '{expectedType}', got '{type}'";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectReturnTypeDiagnostic,
                                message,
                                new List<Range> { node.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        return;

                    case FunctionDeclaration _:
                        return;
                }

                foreach (var child in node.Children())
                {
                    this.CheckReturnTypesMatch(child, expectedType);
                }
            }

            private void DetermineFunctionOverload(FunctionCall funCall)
            {
                var callArgumentsTypes = funCall.Arguments.Select(x => x.Type).ToList();
                var result = funCall.DeclarationCandidates
                    .FirstOrDefault(candidate =>
                    {
                        var candidateParameterTypes = candidate.Parameters.Select(x => x.VariableType).ToList();
                        return FunctionDeclaration.ParametersTypesEquals(candidateParameterTypes, callArgumentsTypes);
                    });
                if (result != null)
                {
                    funCall.Declaration = result;
                }
                else
                {
                    this.ReportOverloadNotFound(funCall);
                }
            }

            private void ReportOverloadNotFound(FunctionCall funCall)
            {
                var message =
                    $"No matching function found for {funCall.Identifier}, found candidates: {string.Join(", ", funCall.DeclarationCandidates)}";
                var ranges = new List<Range> { funCall.InputRange };
                ranges.AddRange(funCall.DeclarationCandidates.Select(x => x.InputRange));

                this.AddDiagnostic(
                    DiagnosticStatus.Error,
                    FunctionOverloadNotFoundDiagnostic,
                    message,
                    ranges);
                this.exceptions.Add(new TypeCheckerInternalException(message));
            }

            private void Dfs(Node node)
            {
                foreach (var child in node.Children())
                {
                    this.Dfs(child);
                }

                switch (node)
                {
                    case Program p:
                        break;

                    case FunctionDeclaration fun:
                        if (fun.ReturnType == null)
                        {
                            var identifier = fun.Identifier;
                            this.exceptions.Add(
                                new TypeCheckerInternalException($"Return type of function {identifier} is null"));
                            throw new TypeCheckerException("Type checking failed.", this.exceptions);
                        }

                        fun.Type = UnitType.Instance;

                        if (!fun.IsForeign)
                            this.CheckReturnTypesMatch(fun.Body, fun.ReturnType);
                        break;

                    case InstructionBlock instruction:
                        instruction.Type = UnitType.Instance;
                        break;

                    case VariableDeclaration variable:
                        if (variable.VariableType == null)
                        {
                            var identifier = variable.Identifier;
                            this.exceptions.Add(
                                new TypeCheckerInternalException($"Type of variable {identifier} is null"));
                            throw new TypeCheckerException("Type checking failed.", this.exceptions);
                        }

                        if (variable.Value != null)
                        {
                            var variableValueType = variable.Value.Type;
                            if (variableValueType == null)
                            {
                                var identifier = variable.Identifier;
                                var message =
                                    $"Incorrect assignment variable '{identifier}' has no type.";
                                this.AddDiagnostic(
                                    DiagnosticStatus.Error,
                                    AssignedValueHasNoTypeDiagnostic,
                                    message,
                                    new List<Range> { variable.InputRange });
                                this.exceptions.Add(new TypeCheckerInternalException(message));
                            }
                            else if (!variable.Value.Type.Equals(variable.VariableType))
                            {
                                var identifier = variable.Identifier;
                                var message =
                                    $"Incorrect assignment value type of '{identifier}'. Expected {variable.VariableType}, got {variable.Value.Type}";
                                this.AddDiagnostic(
                                    DiagnosticStatus.Error,
                                    IncorrectAssigmentTypeDiagnostic,
                                    message,
                                    new List<Range> { variable.InputRange });
                                this.exceptions.Add(new TypeCheckerInternalException(message));
                            }
                        }

                        variable.Type = UnitType.Instance;
                        break;

                    case WhileStatement whileNode:
                        whileNode.Type = UnitType.Instance;
                        break;

                    case IfStatement ifNode:
                        var conditionType = ifNode.Condition.Type;
                        if (!BoolType.Instance.Equals(conditionType))
                        {
                            var message =
                                $"If type must be {BoolType.Instance} got {conditionType}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectIfPredicateTypeDiagnostic,
                                message,
                                new List<Range> { ifNode.Condition.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        ifNode.Type = UnitType.Instance;
                        break;

                    case FunctionCall funCall:
                        this.DetermineFunctionOverload(funCall);
                        funCall.Type = funCall.Declaration?.ReturnType;
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

                    case UnitLiteral unitNode:
                        unitNode.Type = UnitType.Instance;
                        break;

                    case Assignment assignmentNode:
                        if (assignmentNode.Value == null)
                        {
                            var identifier = assignmentNode.Lhs.Identifier;
                            this.exceptions.Add(
                                new TypeCheckerInternalException($"Assignment value of '{identifier}' is null"));
                            throw new TypeCheckerException("Type checking failed.", this.exceptions);
                        }

                        if (!assignmentNode.Lhs.Type.Equals(assignmentNode.Value.Type))
                        {
                            var identifier = assignmentNode.Lhs.Identifier;
                            var message =
                                $"Incorrect assignment value type of '{identifier}'. Expected {assignmentNode.Lhs.Type}, got {assignmentNode.Value.Type}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectAssigmentTypeDiagnostic,
                                message,
                                new List<Range>() { assignmentNode.Lhs.InputRange, assignmentNode.Value.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        assignmentNode.Type = assignmentNode.Lhs.Type;
                        break;

                    case CompoundAssignment compoundNode:
                        if (compoundNode.Value == null)
                        {
                            var identifier = compoundNode.Lhs.Identifier;
                            this.exceptions.Add(
                                new TypeCheckerInternalException($"Compound assignment '{identifier}' value is null"));
                            throw new TypeCheckerException("Type checking failed.", this.exceptions);
                        }

                        if (!compoundNode.Lhs.Type.Equals(IntType.Instance))
                        {
                            var message =
                                $"Incorrect left hand side type: expected {IntType.Instance}, got {compoundNode.Lhs.Type}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectLeftSideTypeDiagnostic,
                                message,
                                new List<Range> { compoundNode.Lhs.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        if (!compoundNode.Value.Type.Equals(IntType.Instance))
                        {
                            var message =
                                $"Incorrect right hand side type: expected {IntType.Instance}, got {compoundNode.Value.Type}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectRightSideTypeDiagnostic,
                                message,
                                new List<Range> { compoundNode.Value.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        compoundNode.Type = compoundNode.Lhs.Type;
                        break;

                    case ArithmeticOperation operationNode:
                        foreach (var operand in new List<Expression>()
                            { operationNode.LeftValue, operationNode.RightValue })
                        {
                            var type = operand.Type;
                            if (type == null)
                                throw new TypeCheckerInternalException("Operand type is null");

                            if (!type.Equals(IntType.Instance))
                            {
                                var message = $"Incorrect operand type: expected {IntType.Instance}, got {type}";
                                this.AddDiagnostic(
                                    DiagnosticStatus.Error,
                                    IncorrectOperandTypeDiagnostic,
                                    message,
                                    new List<Range> { operand.InputRange });
                                this.exceptions.Add(new TypeCheckerInternalException(message));
                            }
                        }

                        operationNode.Type = IntType.Instance;
                        break;

                    case Comparison comparisonNode:
                        if (!comparisonNode.LeftValue.Type.Equals(comparisonNode.RightValue.Type))
                        {
                            var message =
                                $"Comparison type mismatch. Left hand size {comparisonNode.LeftValue.Type}, right hand side {comparisonNode.RightValue.Type}";
                            var ranges = new List<Range> { comparisonNode.InputRange };
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectComparisonTypeDiagnostic,
                                message,
                                ranges);
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        comparisonNode.Type = BoolType.Instance;
                        break;
                    case UnaryOperation unaryOperation:
                        var operation = unaryOperation.UnaryOperationType;
                        var expectedType = UnaryOperationToType[operation];
                        var actualType = unaryOperation.Value.Type;
                        if (expectedType != actualType)
                        {
                            var message =
                                $"Unary operation type mismatch. Operation: '{operation}', expected type: '{expectedType}', actual type: '{actualType}'";
                            var ranges = new List<Range> { unaryOperation.InputRange };
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectUnaryExpressionTypeDiagnostic,
                                message,
                                ranges);
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        unaryOperation.Type = expectedType;
                        break;
                    case LogicalBinaryOperation logicalBinaryOperation:
                        foreach (var operand in new List<Expression>()
                            { logicalBinaryOperation.LeftValue, logicalBinaryOperation.RightValue })
                        {
                            var type = operand.Type;
                            if (type == null)
                            {
                                throw new TypeCheckerInternalException("Operand type is null");
                            }

                            if (!type.Equals(BoolType.Instance))
                            {
                                var message = $"Incorrect operand type: expected {BoolType.Instance}, got {type}";
                                this.AddDiagnostic(
                                    DiagnosticStatus.Error,
                                    IncorrectOperandTypeDiagnostic,
                                    message,
                                    new List<Range> { operand.InputRange });
                                this.exceptions.Add(new TypeCheckerInternalException(message));
                            }
                        }

                        logicalBinaryOperation.Type = BoolType.Instance;
                        break;
                    case BreakStatement _:
                        break;
                    case ArrayAlloc arrayAlloc:
                        var elementType = arrayAlloc.ElementType;
                        var sizeType = arrayAlloc.Size.Type;
                        if (elementType == null)
                        {
                            throw new TypeCheckerInternalException("Array type is null");
                        }

                        if (sizeType == null)
                        {
                            throw new TypeCheckerInternalException("Size type is null");
                        }
                        else if (!sizeType.Equals(IntType.Instance))
                        {
                            var message = $"Incorrect array size type: expected {IntType.Instance}, got {sizeType}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectArraySizeTypeDiagnostic,
                                message,
                                new List<Range> { arrayAlloc.Size.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        arrayAlloc.Type = ArrayType.GetInstance(elementType);
                        break;
                    case ArrayAccess arrayAccess:
                        var array = arrayAccess.Lhs;
                        var index = arrayAccess.Index;

                        if (array.Type == null)
                        {
                            throw new TypeCheckerInternalException("Array type is null");
                        }
                        else if (!(array.Type is ArrayType))
                        {
                            var message = $"Expected array type, got {array.Type}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectArrayAccessUseDiagnostic,
                                message,
                                new List<Range> { array.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        if (index.Type == null)
                        {
                            throw new TypeCheckerInternalException("Index type is null");
                        }
                        else if (!index.Type.Equals(IntType.Instance))
                        {
                            var message = $"Incorrect array index type: expected {IntType.Instance}, got {index.Type}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectArrayIndexTypeDiagnostic,
                                message,
                                new List<Range> { index.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        arrayAccess.Type = (array.Type as ArrayType)?.ElementType;
                        break;
                    case ArrayAssignment arrayAssignment:
                        var elType = arrayAssignment.Lhs.Type;
                        var valueType = arrayAssignment.Value.Type;

                        if (elType == null)
                        {
                            throw new TypeCheckerInternalException("Element type is null");
                        }

                        if (valueType == null)
                        {
                            throw new TypeCheckerInternalException("Value type is null");
                        }

                        if (!elType.Equals(valueType))
                        {
                            var message = $"Array assignment type mismatch: {elType}, value type {valueType}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectArrayTypeDiagnostic,
                                message,
                                new List<Range> { arrayAssignment.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        arrayAssignment.Type = elType;
                        break;
                    case ArrayCompoundAssignment arrayCompoundAssignment:
                        var elType2 = arrayCompoundAssignment.Lhs.Type;
                        var valueType2 = arrayCompoundAssignment.Value.Type;

                        if (elType2 == null)
                        {
                            throw new TypeCheckerInternalException("Element type is null");
                        }
                        else if (!elType2.Equals(IntType.Instance))
                        {
                            var message = $"Array compound assignment element type mismatch: got {elType2}, expected {IntType.Instance}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectArrayTypeDiagnostic,
                                message,
                                new List<Range> { arrayCompoundAssignment.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        if (valueType2 == null)
                        {
                            throw new TypeCheckerInternalException("Value type is null");
                        }
                        else if (!valueType2.Equals(IntType.Instance))
                        {
                            var message = $"Array compound assignment value type mismatch: got {valueType2}, expected {IntType.Instance}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectArrayTypeDiagnostic,
                                message,
                                new List<Range> { arrayCompoundAssignment.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        arrayCompoundAssignment.Type = IntType.Instance;
                        break;
                    case Expression e:
                        this.exceptions.Add(
                            new TypeCheckerInternalException($"Unrecognized node type: {node.GetType()}"));
                        throw new TypeCheckerException("Type checking failed.", this.exceptions);
                }
            }
        }
    }
}