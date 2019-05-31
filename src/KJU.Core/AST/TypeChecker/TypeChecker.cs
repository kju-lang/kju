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
        public const string IncorrectArrayAccessUseDiagnostic = "TypeChecker.IncorrectArrayAccessUse";
        public const string AssignedValueHasNoTypeDiagnostic = "TypeChecker.AssignedValueHasNoType";
        public const string FunctionOverloadNotFoundDiagnostic = "TypeChecker.FunctionOverloadNotFound";
        public const string IncorrectStructTypeDiagnostic = "TypeChecker.IncorrectStructType";
        public const string IncorrectFieldNameDiagnostic = "TypeChecker.IncorrectFieldName";

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
                        if (!this.CanBeConverted(type, expectedType))
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

            private bool CanBeConverted(DataType from, DataType to)
            {
                if (from == null || to == null)
                {
                    return false;
                }

                if (from.Equals(NullType.Instance))
                {
                    return (to is NullType) || (to is ArrayType) || (to is StructType);
                }
                else
                {
                    return from.Equals(to);
                }
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
                    {
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
                    }

                    case InstructionBlock instruction:
                    {
                        instruction.Type = UnitType.Instance;
                        break;
                    }

                    case VariableDeclaration variable:
                    {
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
                            else if (!this.CanBeConverted(variableValueType, variable.VariableType))
                            {
                                var identifier = variable.Identifier;
                                var message =
                                    $"Incorrect assignment value type of '{identifier}'. Expected {variable.VariableType}, got {variableValueType}";
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
                    }

                    case WhileStatement whileNode:
                    {
                        whileNode.Type = UnitType.Instance;
                        break;
                    }

                    case IfStatement ifNode:
                    {
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
                    }

                    case FunctionCall funCall:
                    {
                        this.DetermineFunctionOverload(funCall);
                        funCall.Type = funCall.Declaration?.ReturnType;
                        break;
                    }

                    case ReturnStatement returnNode:
                    {
                        returnNode.Type = returnNode.Value == null ? UnitType.Instance : returnNode.Value.Type;
                        break;
                    }

                    case Variable variable:
                    {
                        variable.Type = variable.Declaration.VariableType;
                        break;
                    }

                    case BoolLiteral boolNode:
                    {
                        boolNode.Type = BoolType.Instance;
                        break;
                    }

                    case IntegerLiteral integerNode:
                    {
                        integerNode.Type = IntType.Instance;
                        break;
                    }

                    case UnitLiteral unitNode:
                    {
                        unitNode.Type = UnitType.Instance;
                        break;
                    }

                    case NullLiteral nullNode:
                    {
                        nullNode.Type = NullType.Instance;
                        break;
                    }

                    case Assignment assignmentNode:
                    {
                        if (assignmentNode.Value == null)
                        {
                            var identifier = assignmentNode.Lhs.Identifier;
                            this.exceptions.Add(
                                new TypeCheckerInternalException($"Assignment value of '{identifier}' is null"));
                            throw new TypeCheckerException("Type checking failed.", this.exceptions);
                        }

                        if (!this.CanBeConverted(assignmentNode.Value.Type, assignmentNode.Lhs.Type))
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
                    }

                    case CompoundAssignment compoundNode:
                    {
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
                    }

                    case ArithmeticOperation operationNode:
                    {
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
                    }

                    case Comparison comparisonNode:
                    {
                        var lhsType = comparisonNode.LeftValue.Type;
                        var rhsType = comparisonNode.RightValue.Type;
                        var opType = comparisonNode.OperationType;

                        bool canBeCompared;
                        if (opType.Equals(ComparisonType.Equal) || opType.Equals(ComparisonType.NotEqual))
                        {
                            canBeCompared = this.CanBeConverted(lhsType, rhsType) || this.CanBeConverted(rhsType, lhsType);
                        }
                        else
                        {
                            canBeCompared = lhsType.Equals(rhsType);
                        }

                        if (!canBeCompared)
                        {
                            var message =
                                $"Comparison type mismatch. Left hand size {lhsType}, right hand side {rhsType}";
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
                    }

                    case UnaryOperation unaryOperation:
                    {
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
                    }

                    case LogicalBinaryOperation logicalBinaryOperation:
                    {
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
                    }

                    case BreakStatement _:
                        break;

                    case ArrayAlloc arrayAlloc:
                    {
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
                    }

                    case ArrayAccess arrayAccess:
                    {
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
                    }

                    case ComplexAssignment complexAssignment:
                    {
                        var lhsType = complexAssignment.Lhs.Type;
                        var valueType = complexAssignment.Value.Type;

                        if (lhsType == null)
                        {
                            throw new TypeCheckerInternalException("Left hand side type is null");
                        }

                        if (valueType == null)
                        {
                            throw new TypeCheckerInternalException("Value type is null");
                        }

                        if (!this.CanBeConverted(valueType, lhsType))
                        {
                            var message = $"Assignment type mismatch: {lhsType}, value type {valueType}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectAssigmentTypeDiagnostic,
                                message,
                                new List<Range> { complexAssignment.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        complexAssignment.Type = lhsType;
                        break;
                    }

                    case ComplexCompoundAssignment complexCompoundAssignment:
                    {
                        var lhsType = complexCompoundAssignment.Lhs.Type;
                        var valueType = complexCompoundAssignment.Value.Type;

                        if (lhsType == null)
                        {
                            throw new TypeCheckerInternalException("Left hand side type is null");
                        }
                        else if (!lhsType.Equals(IntType.Instance))
                        {
                            var message = $"Compound assignment left hand side type mismatch: got {lhsType}, expected {IntType.Instance}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectLeftSideTypeDiagnostic,
                                message,
                                new List<Range> { complexCompoundAssignment.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        if (valueType == null)
                        {
                            throw new TypeCheckerInternalException("Value type is null");
                        }
                        else if (!valueType.Equals(IntType.Instance))
                        {
                            var message = $"Compound assignment value type mismatch: got {valueType}, expected {IntType.Instance}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectRightSideTypeDiagnostic,
                                message,
                                new List<Range> { complexCompoundAssignment.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }

                        complexCompoundAssignment.Type = IntType.Instance;
                        break;
                    }

                    case FieldAccess fieldAccess:
                    {
                        var lhsType = fieldAccess.Lhs.Type;

                        if (lhsType == null)
                        {
                            throw new TypeCheckerInternalException("Struct type is null");
                        }
                        else if (!(lhsType is StructType))
                        {
                            var message = $"Expected struct type, got {lhsType}";
                            this.AddDiagnostic(
                                DiagnosticStatus.Error,
                                IncorrectStructTypeDiagnostic,
                                message,
                                new List<Range> { fieldAccess.Lhs.InputRange });
                            this.exceptions.Add(new TypeCheckerInternalException(message));
                        }
                        else
                        {
                            var fieldName = fieldAccess.Field;
                            var allFields = (lhsType as StructType).Fields;
                            var matchingFields = allFields.Where(field => field.Name.Equals(fieldName));

                            var numberOfMatchingField = matchingFields.Count();
                            if (numberOfMatchingField == 0)
                            {
                                var message =
                                    $"Incorrect field name: expected one of {allFields}, got {fieldName}";
                                this.AddDiagnostic(
                                    DiagnosticStatus.Error,
                                    IncorrectFieldNameDiagnostic,
                                    message,
                                    new List<Range> { fieldAccess.InputRange });
                                this.exceptions.Add(new TypeCheckerInternalException(message));
                            }
                            else if (numberOfMatchingField > 1)
                            {
                                throw new TypeCheckerInternalException($"Many fields matching name {fieldName}");
                            }
                            else
                            {
                                var fieldType = matchingFields.First().Type;
                                if (fieldType == null)
                                {
                                    throw new TypeCheckerInternalException("Field type is null");
                                }

                                fieldAccess.Type = fieldType;
                            }
                        }

                        break;
                    }

                    case StructDeclaration structDeclaration:
                    {
                        structDeclaration.Type = UnitType.Instance;
                        break;
                    }

                    case StructAlloc structAlloc:
                    {
                        var structType = StructType.GetInstance(structAlloc.Declaration);

                        if (structType == null)
                        {
                            throw new TypeCheckerInternalException("Structure type for struct declaration is null");
                        }
                        else
                        {
                            structAlloc.Type = structType;
                        }

                        break;
                    }

                    case Expression e:
                    {
                        this.exceptions.Add(
                            new TypeCheckerInternalException($"Unrecognized node type: {node.GetType()}"));
                        throw new TypeCheckerException("Type checking failed.", this.exceptions);
                    }
                }
            }
        }
    }
}
