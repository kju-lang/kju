namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BuiltinTypes;
    using Diagnostics;
    using Types;

    using Alternative = System.Collections.Generic.List<(Types.IHerbrandObject, Types.IHerbrandObject)>;

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
        public const string IncorrectApplicationArgsDiagnostic = "TypeChecker.IncorrectApplicationArgs";
        public const string IncorrectApplicationFuncDiagnostic = "TypeChecker.IncorrectApplicationFunc";
        public const string AmbiguousUnapplicationDiagnostic = "TypeChecker.AmbiguousUnapplication";

        public const string TypeAssignmentDiagnostic = "TypeChecker.TypeAssignmentFail";

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
            private readonly Stack<DataType> returnType = new Stack<DataType>();
            private readonly List<Clause> clauses = new List<Clause>();
            private readonly Dictionary<Expression, TypeVariable> expressionTypes = new Dictionary<Expression, TypeVariable>();
            private readonly Dictionary<FunctionCall, Clause> callOptions = new Dictionary<FunctionCall, Clause>();
            private readonly Dictionary<UnApplication, Clause> unappOptions = new Dictionary<UnApplication, Clause>();

            public TypeCheckerProcess(IDiagnostics diagnostics)
            {
                this.diagnostics = diagnostics;
            }

            public void LinkTypes(Node root)
            {
                this.ProcessChildren(root);

                try
                {
                    Solution solution = new Solver(this.clauses).Solve();
                    Solution normalizedSolution = SolutionNormalizer.Normalize(solution);
                    this.SubstituteSolution(root, normalizedSolution);
                }
                catch (Exception ex) when (ex is TypeCheckerException || ex is SolutionNormalizerException)
                {
                    this.diagnostics.Add(new Diagnostic(
                        DiagnosticStatus.Error,
                        TypeAssignmentDiagnostic,
                        "Could not assign types",
                        new List<Lexer.Range>()));
                    throw new TypeCheckerException("Could not assign types", ex);
                }
            }

            private void ProcessChildren(Node node)
            {
                if (node is FunctionDeclaration declaration) this.returnType.Push(declaration.ReturnType);

                foreach (var child in node.Children())
                {
                    if (child is Expression e)
                    {
                        this.clauses.AddRange(this.GenerateClauses(e));
                    }

                    this.ProcessChildren(child);
                }

                if (node is FunctionDeclaration) this.returnType.Pop();
            }

            private Clause EqualityClause(IHerbrandObject a, IHerbrandObject b, Node node)
            {
                return new Clause((a, b), node.InputRange);
            }

            private Clause EqualityClause(Expression node, IHerbrandObject t)
            {
                return new Clause((node.Type, t), node.InputRange);
            }

            private (IHerbrandObject, IHerbrandObject) MatchArgument(Expression arg, VariableDeclaration param)
            {
                return (arg.Type, param.VariableType);
            }

            private Clause OverloadsClause(FunctionCall call)
            {
                Func<FunctionDeclaration, Alternative> alternative = decl =>
                    Enumerable.Zip(call.Arguments, decl.Parameters, this.MatchArgument)
                        .Append((call.Type, decl.ReturnType))
                        .ToList();
                var alternatives = call.DeclarationCandidates.Select(alternative).ToList();
                return new Clause(alternatives, call.InputRange);
            }

            private Clause StructCandidatesClause(FieldAccess access)
            {
                Func<KeyValuePair<StructDeclaration, StructField>, Alternative> alternative = decl =>
                    new Alternative
                    {
                        (access.Lhs.Type, decl.Key.StructType),
                        (access.Type, decl.Value.Type),
                    };
                var alternatives = access.StructCandidates.Select(alternative).ToList();
                return new Clause(alternatives, access.InputRange);
            }

            private IEnumerable<Clause> GenerateClauses(Expression node)
            {
                if (!(node.Type is TypeVariable))
                {
                    this.expressionTypes[node] = new TypeVariable();
                    yield return this.EqualityClause(node, this.expressionTypes[node]);
                }

                switch (node)
                {
                    case NullLiteral _:
                        break;

                    case InstructionBlock _:
                    case FunctionDeclaration _:
                    case StructDeclaration _:
                    case BreakStatement _:
                    case ContinueStatement _:
                        yield return this.EqualityClause(node, UnitType.Instance);
                        break;

                    case VariableDeclaration decl:
                        if (decl.Value != null)
                        {
                            yield return this.EqualityClause(decl.Value, decl.VariableType);
                        }

                        yield return this.EqualityClause(node, UnitType.Instance);
                        break;

                    case WhileStatement whileNode:
                        yield return this.EqualityClause(whileNode.Condition, BoolType.Instance);
                        yield return this.EqualityClause(node, UnitType.Instance);
                        break;

                    case IfStatement ifNode:
                        yield return this.EqualityClause(ifNode.Condition, BoolType.Instance);
                        yield return this.EqualityClause(node, UnitType.Instance);
                        break;

                    case FunctionCall funCall:
                    {
                        Clause clause = this.OverloadsClause(funCall);
                        this.callOptions[funCall] = clause;
                        yield return clause;
                        break;
                    }

                    case ReturnStatement returnNode:
                        yield return returnNode.Value == null
                            ? this.EqualityClause(this.returnType.Peek(), UnitType.Instance, returnNode)
                            : this.EqualityClause(returnNode.Value, this.returnType.Peek());
                        yield return this.EqualityClause(returnNode, returnNode.Value?.Type ?? UnitType.Instance);
                        break;

                    case Variable variable:
                        yield return this.EqualityClause(variable, variable.Declaration.VariableType);
                        break;

                    case BoolLiteral boolNode:
                        yield return this.EqualityClause(boolNode, BoolType.Instance);
                        break;

                    case IntegerLiteral integerNode:
                        yield return this.EqualityClause(integerNode, IntType.Instance);
                        break;

                    case UnitLiteral unitNode:
                        yield return this.EqualityClause(unitNode, UnitType.Instance);
                        break;

                    case Assignment assignment:
                        yield return this.EqualityClause(assignment.Value, assignment.Lhs.Declaration.VariableType);
                        yield return this.EqualityClause(assignment, assignment.Value.Type);
                        break;

                    case CompoundAssignment assignment:
                        yield return this.EqualityClause(assignment.Lhs.Declaration.VariableType, IntType.Instance, assignment.Lhs);
                        yield return this.EqualityClause(assignment.Value, IntType.Instance);
                        yield return this.EqualityClause(assignment, IntType.Instance);
                        break;

                    case ArithmeticOperation operationNode:
                        yield return this.EqualityClause(operationNode.LeftValue, IntType.Instance);
                        yield return this.EqualityClause(operationNode.RightValue, IntType.Instance);
                        yield return this.EqualityClause(operationNode, IntType.Instance);
                        break;

                    case Comparison cmp:
                    {
                        switch (cmp.OperationType)
                        {
                            case ComparisonType.Equal:
                            case ComparisonType.NotEqual:
                                yield return this.EqualityClause(cmp.LeftValue.Type, cmp.RightValue.Type, cmp);
                                break;
                            case ComparisonType.Less:
                            case ComparisonType.LessOrEqual:
                            case ComparisonType.Greater:
                            case ComparisonType.GreaterOrEqual:
                                yield return this.EqualityClause(cmp.LeftValue, IntType.Instance);
                                yield return this.EqualityClause(cmp.RightValue, IntType.Instance);
                                break;
                        }

                        yield return this.EqualityClause(cmp, BoolType.Instance);
                        break;
                    }

                    case UnaryOperation op:
                    {
                        IHerbrandObject type = UnaryOperationToType[op.UnaryOperationType];
                        yield return this.EqualityClause(op.Value, type);
                        yield return this.EqualityClause(op, type);
                        break;
                    }

                    case LogicalBinaryOperation op:
                        yield return this.EqualityClause(op.LeftValue, BoolType.Instance);
                        yield return this.EqualityClause(op.RightValue, BoolType.Instance);
                        yield return this.EqualityClause(op, BoolType.Instance);
                        break;

                    case ArrayAlloc alloc:
                        yield return this.EqualityClause(alloc.Size, IntType.Instance);
                        yield return this.EqualityClause(alloc, new ArrayType(alloc.ElementType));
                        break;

                    case ArrayAccess access:
                        yield return this.EqualityClause(access.Index, IntType.Instance);
                        yield return this.EqualityClause(access.Lhs, new ArrayType(access.Type));
                        break;

                    case ComplexAssignment assignment:
                        yield return this.EqualityClause(assignment.Lhs, assignment.Value.Type);
                        yield return this.EqualityClause(assignment, assignment.Value.Type);
                        break;

                    case ComplexCompoundAssignment assignment:
                        yield return this.EqualityClause(assignment.Lhs, IntType.Instance);
                        yield return this.EqualityClause(assignment.Value, IntType.Instance);
                        yield return this.EqualityClause(assignment, IntType.Instance);
                        break;

                    case FieldAccess access:
                        yield return this.StructCandidatesClause(access);
                        break;

                    case StructAlloc alloc:
                        yield return this.EqualityClause(alloc, StructType.GetInstance(alloc.Declaration));
                        break;

                    case Application app:
                        yield return this.EqualityClause(
                            app.Function,
                            new FunType(app.Arguments.Select(arg => arg.Type), app.Type));
                        break;

                    case UnApplication unapp:
                    {
                        Clause clause = new Clause(
                            unapp.Candidates
                                .Select(decl => new Alternative { (unapp.Type, new FunType(decl)) })
                                .ToList(), unapp.InputRange);
                        this.unappOptions[unapp] = clause;
                        yield return clause;
                        break;
                    }

                    case Expression e:
                        throw new TypeCheckerException($"Unrecognized node type: {node.GetType()}");
                }
            }

            private void SubstituteSolution(Node node, Solution solution)
            {
                this.FillChoice(node, solution);
                if (node is Expression e)
                {
                    var typeVar = (e.Type as TypeVariable) ?? this.expressionTypes[e];
                    e.Type = solution.TypeVariableMapping[typeVar] as DataType;
                }

                foreach (var child in node.Children())
                {
                    this.SubstituteSolution(child, solution);
                }
            }

            private void FillChoice(Node node, Solution solution)
            {
                switch (node)
                {
                    case FunctionCall call:
                    {
                        int ix = solution.ChosenAlternative[this.callOptions[call]];
                        call.Declaration = call.DeclarationCandidates[ix];
                        break;
                    }

                    case UnApplication unapp:
                    {
                        int ix = solution.ChosenAlternative[this.unappOptions[unapp]];
                        unapp.Declaration = unapp.Candidates[ix];
                        break;
                    }
                }
            }
        }
    }
}
