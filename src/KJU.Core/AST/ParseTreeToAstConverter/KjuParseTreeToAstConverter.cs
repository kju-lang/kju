namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Diagnostics;
    using Lexer;
    using Parser;

    public class KjuParseTreeToAstConverter : IParseTreeToAstConverter<KjuAlphabet>
    {
        public static readonly string TokenCategoryErrorDiagnosticsType = "unexpectedTokenCategory";
        public static readonly string TypeIdentifierErrorDiagnosticsType = "unexpectedTypeIdentifier";
        public static readonly string AstConversionErrorDiagnosticsType = "astConversionError";
        public static readonly string AssignmentLhsErrorDiagnosticsType = "assignmentLhsError";

        private readonly Dictionary<KjuAlphabet, ArithmeticOperationType> symbolToOperationType;
        private readonly Dictionary<KjuAlphabet, ComparisonType> symbolToComparisonType;
        private readonly Dictionary<KjuAlphabet, UnaryOperationType> symbolToUnaryOperationType;

        public KjuParseTreeToAstConverter()
        {
            this.symbolToOperationType = new Dictionary<KjuAlphabet, ArithmeticOperationType>()
            {
                // assigment operators
                [KjuAlphabet.PlusAssign] = ArithmeticOperationType.Addition,
                [KjuAlphabet.MinusAssign] = ArithmeticOperationType.Subtraction,
                [KjuAlphabet.StarAssign] = ArithmeticOperationType.Multiplication,
                [KjuAlphabet.SlashAssign] = ArithmeticOperationType.Division,
                [KjuAlphabet.PercentAssign] = ArithmeticOperationType.Remainder,

                // regular operators
                [KjuAlphabet.Plus] = ArithmeticOperationType.Addition,
                [KjuAlphabet.Minus] = ArithmeticOperationType.Subtraction,
                [KjuAlphabet.Star] = ArithmeticOperationType.Multiplication,
                [KjuAlphabet.Slash] = ArithmeticOperationType.Division,
                [KjuAlphabet.Percent] = ArithmeticOperationType.Remainder
            };

            this.symbolToComparisonType = new Dictionary<KjuAlphabet, ComparisonType>()
            {
                [KjuAlphabet.Equals] = ComparisonType.Equal,
                [KjuAlphabet.NotEquals] = ComparisonType.NotEqual,
                [KjuAlphabet.LessThan] = ComparisonType.Less,
                [KjuAlphabet.LessOrEqual] = ComparisonType.LessOrEqual,
                [KjuAlphabet.GreaterThan] = ComparisonType.Greater,
                [KjuAlphabet.GreaterOrEqual] = ComparisonType.GreaterOrEqual,
            };

            this.symbolToUnaryOperationType = new Dictionary<KjuAlphabet, UnaryOperationType>
            {
                [KjuAlphabet.LogicNot] = UnaryOperationType.Not,
                [KjuAlphabet.Plus] = UnaryOperationType.Plus,
                [KjuAlphabet.Minus] = UnaryOperationType.Minus
            };
        }

        public Node GenerateAst(ParseTree<KjuAlphabet> parseTree, IDiagnostics diagnostics)
        {
            return new ConverterProcess(
                    this.symbolToOperationType,
                    this.symbolToComparisonType,
                    this.symbolToUnaryOperationType)
                .GenerateAst(parseTree, diagnostics);
        }

        private class ConverterProcess
        {
            private readonly Dictionary<KjuAlphabet, ArithmeticOperationType> symbolToOperationType;
            private readonly Dictionary<KjuAlphabet, ComparisonType> symbolToComparisonType;
            private readonly Dictionary<KjuAlphabet, UnaryOperationType> symbolToUnaryOperationType;

            private readonly Dictionary<KjuAlphabet, Func<Brunch<KjuAlphabet>, IDiagnostics, Expression>>
                symbolToGenFunction;

            private readonly HashSet<Node> enclosedWithParentheses = new HashSet<Node>();

            public ConverterProcess(
                Dictionary<KjuAlphabet, ArithmeticOperationType> symbolToOperationType,
                Dictionary<KjuAlphabet, ComparisonType> symbolToComparisonType,
                Dictionary<KjuAlphabet, UnaryOperationType> symbolToUnaryOperationType)
            {
                this.symbolToOperationType = symbolToOperationType;
                this.symbolToComparisonType = symbolToComparisonType;
                this.symbolToUnaryOperationType = symbolToUnaryOperationType;
                this.symbolToGenFunction =
                    new Dictionary<KjuAlphabet, Func<Brunch<KjuAlphabet>, IDiagnostics, Expression>>()
                    {
                        [KjuAlphabet.FunctionDeclaration] = this.FunctionDeclarationToAst,
                        [KjuAlphabet.Block] = this.BlockToAst,
                        [KjuAlphabet.Instruction] = this.InstructionToAst,
                        [KjuAlphabet.NotDelimeteredInstruction] = this.NotDelimeteredInstructionToAst,
                        [KjuAlphabet.FunctionParameter] = this.FunctionParameterToAst,
                        [KjuAlphabet.IfStatement] = this.IfStatementToAst,
                        [KjuAlphabet.WhileStatement] = this.WhileStatementToAst,
                        [KjuAlphabet.ReturnStatement] = this.ReturnStatementToAst,
                        [KjuAlphabet.VariableDeclaration] = this.VariableDeclarationToAst,
                        [KjuAlphabet.VariableUse] = this.VariableUseToAst,
                        [KjuAlphabet.Statement] = this.StatementToAst,
                        [KjuAlphabet.Expression] = this.ExpressionToAst,
                        [KjuAlphabet.ExpressionAssignment] = this.ExpressionAssignmentToAst,
                        [KjuAlphabet.ExpressionOr] = this.ExpressionLogicalOrToAst,
                        [KjuAlphabet.ExpressionAnd] = this.ExpressionLogicalAndToAst,
                        [KjuAlphabet.ExpressionEqualsNotEquals] = this.ExpressionEqualsNotEqualsToAst,
                        [KjuAlphabet.ExpressionLessThanGreaterThan] = this.ExpressionLessThanGreaterThanToAst,
                        [KjuAlphabet.ExpressionPlusMinus] = this.ExpressionPlusMinusToAst,
                        [KjuAlphabet.ExpressionTimesDivideModulo] = this.ExpressionTimesDivideModuloToAst,
                        [KjuAlphabet.ExpressionUnaryOperator] = this.ExpressionLogicalNotToAst,
                        [KjuAlphabet.Literal] = this.ExpressionAtomToAst,
                    };
            }

            public Node GenerateAst(ParseTree<KjuAlphabet> parseTree, IDiagnostics diagnostics)
            {
                this.enclosedWithParentheses.Clear();
                var branch = (Brunch<KjuAlphabet>)parseTree;
                var functions = branch.Children
                    .Cast<Brunch<KjuAlphabet>>()
                    .Select(child => this.FunctionDeclarationToAst(child, diagnostics))
                    .ToList();
                var ast = new Program(functions);

                this.FlipToLeftAssignmentAst(ast);
                this.enclosedWithParentheses.Clear();
                return ast;
            }

            private static void SwapOps(BinaryOperation first, BinaryOperation second)
            {
                switch (first)
                {
                    case ArithmeticOperation firstArithmeticOperation:
                        var arithmeticType = firstArithmeticOperation.OperationType;
                        var secondArithmeticOperation = (ArithmeticOperation)second;
                        firstArithmeticOperation.OperationType = secondArithmeticOperation.OperationType;
                        secondArithmeticOperation.OperationType = arithmeticType;
                        break;
                    case Comparison firstComparision:
                        var comparisonType = firstComparision.OperationType;
                        var secondComparision = (Comparison)second;
                        firstComparision.OperationType = secondComparision.OperationType;
                        secondComparision.OperationType = comparisonType;
                        break;
                    case LogicalBinaryOperation firstLogicOperation:
                        var logicType = firstLogicOperation.BinaryOperationType;
                        var secondLogicOperation = (LogicalBinaryOperation)second;
                        firstLogicOperation.BinaryOperationType = secondLogicOperation.BinaryOperationType;
                        secondLogicOperation.BinaryOperationType = logicType;
                        break;
                }
            }

            private static void SwapOperationsInPath(IReadOnlyList<BinaryOperation> path)
            {
                var n = path.Count;
                for (var i = 0; i < n / 2; i++)
                {
                    SwapOps(path[i], path[n - (i + 1)]);
                }
            }

            private static Expression TokenAst(Token<KjuAlphabet> token, IDiagnostics diagnostics)
            {
                switch (token.Category)
                {
                    case KjuAlphabet.Break:
                        return new BreakStatement();
                    case KjuAlphabet.Continue:
                        return new ContinueStatement();
                    case KjuAlphabet.DecimalLiteral:
                        var intValue = long.Parse(token.Text);
                        return new IntegerLiteral(intValue);
                    case KjuAlphabet.BooleanLiteral:
                        var boolValue = bool.Parse(token.Text);
                        return new BoolLiteral(boolValue);
                    default:
                        var diag = new Diagnostic(
                            DiagnosticStatus.Error,
                            TokenCategoryErrorDiagnosticsType,
                            $"{{0}} Unexpected token category: {token.Category}",
                            new List<Range> { token.InputRange });
                        diagnostics.Add(diag);
                        throw new ParseTreeToAstConverterException("Unexpected category in token");
                }
            }

            private void FlipToLeftAssignmentAst(Node ast)
            {
                var arithmeticOrder = new Dictionary<ArithmeticOperationType, int>
                {
                    [ArithmeticOperationType.Addition] = 0,
                    [ArithmeticOperationType.Subtraction] = 0,
                    [ArithmeticOperationType.Multiplication] = 1,
                    [ArithmeticOperationType.Division] = 1,
                    [ArithmeticOperationType.Remainder] = 1,
                };

                if (this.enclosedWithParentheses.Contains(ast))
                {
                    this.enclosedWithParentheses.Remove(ast);
                }

                if (ast is BinaryOperation root)
                {
                    var path = new List<BinaryOperation>();
                    var danglingNodes = new List<Expression>();
                    var current = root;
                    while (current.GetType() == root.GetType())
                    {
                        if (this.enclosedWithParentheses.Contains(current))
                        {
                            break;
                        }

                        if (current is ArithmeticOperation currentOp && root is ArithmeticOperation rootOp)
                        {
                            if (arithmeticOrder[currentOp.OperationType] != arithmeticOrder[rootOp.OperationType])
                            {
                                break;
                            }
                        }

                        path.Add(current);
                        this.FlipToLeftAssignmentAst(current.LeftValue);
                        danglingNodes.Add(current.LeftValue);
                        if (!(current.RightValue is BinaryOperation))
                        {
                            break;
                        }

                        current = (BinaryOperation)current.RightValue;
                    }

                    var n = path.Count;
                    this.FlipToLeftAssignmentAst(path[n - 1].RightValue);
                    danglingNodes.Add(path[n - 1].RightValue);

                    // path[0] == root
                    for (var i = 0; i < n - 1; i++)
                    {
                        path[i].LeftValue = path[i + 1];
                        path[i].RightValue = danglingNodes[n - i];
                    }

                    path[n - 1].LeftValue = danglingNodes[0];
                    path[n - 1].RightValue = danglingNodes[1];
                    SwapOperationsInPath(path);
                }
                else
                {
                    foreach (var child in ast.Children())
                    {
                        this.FlipToLeftAssignmentAst(child);
                    }
                }
            }

            private Expression GeneralToAst(ParseTree<KjuAlphabet> parseTree, IDiagnostics diagnostics)
            {
                if (this.symbolToGenFunction.ContainsKey(parseTree.Category))
                {
                    return this.symbolToGenFunction[parseTree.Category](parseTree as Brunch<KjuAlphabet>, diagnostics);
                }
                else
                {
                    return TokenAst((Token<KjuAlphabet>)parseTree, diagnostics);
                }
            }

            private DataType TypeIdentifierAst(Token<KjuAlphabet> token, IDiagnostics diagnostics)
            {
                switch (token.Text)
                {
                    case "Bool":
                        return BuiltinTypes.BoolType.Instance;
                    case "Int":
                        return BuiltinTypes.IntType.Instance;
                    case "Unit":
                        return BuiltinTypes.UnitType.Instance;
                    default:
                        var diag = new Diagnostic(
                            DiagnosticStatus.Error,
                            TypeIdentifierErrorDiagnosticsType,
                            $"{{0}} Unexpected type identifier: '{token.Text}'",
                            new List<Range> { token.InputRange });
                        diagnostics.Add(diag);
                        throw new ParseTreeToAstConverterException("unexpected type identifier");
                }
            }

            private FunctionDeclaration FunctionDeclarationToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var parameters = new List<VariableDeclaration>();
                string identifier = null;
                DataType type = null;
                InstructionBlock body = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.FunctionParameter:
                            parameters.Add(this.FunctionParameterToAst((Brunch<KjuAlphabet>)child, diagnostics));
                            break;
                        case KjuAlphabet.VariableFunctionIdentifier:
                            identifier = ((Token<KjuAlphabet>)child).Text;
                            break;
                        case KjuAlphabet.TypeIdentifier:
                            type = this.TypeIdentifierAst((Token<KjuAlphabet>)child, diagnostics);
                            break;
                        case KjuAlphabet.Block:
                            body = this.BlockToAst((Brunch<KjuAlphabet>)child, diagnostics);
                            break;
                    }
                }

                var ast = new FunctionDeclaration(
                    identifier,
                    type,
                    parameters,
                    body);

                return ast;
            }

            private InstructionBlock BlockToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var instructions = branch.Children
                    .Where(child => child.Category == KjuAlphabet.Instruction)
                    .Cast<Brunch<KjuAlphabet>>()
                    .Select(child => this.InstructionToAst(child, diagnostics))
                    .ToList();
                return new InstructionBlock(instructions);
            }

            private Expression InstructionToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                return branch.Children.Count == 1 ? new UnitLiteral() : this.NotDelimeteredInstructionToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
            }

            private Expression NotDelimeteredInstructionToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                return this.StatementToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
            }

            private VariableDeclaration FunctionParameterToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                string identifier = null;
                DataType type = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.VariableFunctionIdentifier:
                            identifier = ((Token<KjuAlphabet>)child).Text;
                            break;
                        case KjuAlphabet.TypeIdentifier:
                            type = this.TypeIdentifierAst((Token<KjuAlphabet>)child, diagnostics);
                            break;
                    }
                }

                return new VariableDeclaration(type, identifier, null);
            }

            private List<Expression> FunctionCallArgumentsToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var arguments = branch.Children
                    .Where(child => child.Category == KjuAlphabet.Expression)
                    .Cast<Brunch<KjuAlphabet>>()
                    .Select(child => this.ExpressionToAst(child, diagnostics))
                    .ToList();
                return arguments;
            }

            private IfStatement IfStatementToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var blockList = new List<InstructionBlock>();
                Expression condition = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.Expression:
                            condition = this.ExpressionToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            break;
                        case KjuAlphabet.Block:
                            blockList.Add(this.BlockToAst(child as Brunch<KjuAlphabet>, diagnostics));
                            break;
                    }
                }

                return new IfStatement(condition, blockList[0], blockList[1]);
            }

            private WhileStatement WhileStatementToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression condition = null;
                InstructionBlock body = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.Expression:
                            condition = this.ExpressionToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            break;
                        case KjuAlphabet.Block:
                            body = this.BlockToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            break;
                    }
                }

                return new WhileStatement(condition, body);
            }

            private Expression ReturnStatementToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression value = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.Expression:
                            value = this.ExpressionToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            break;
                    }
                }

                return new ReturnStatement(value);
            }

            private VariableDeclaration VariableDeclarationToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                string identifier = null;
                DataType type = null;
                Expression value = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.VariableFunctionIdentifier:
                            identifier = ((Token<KjuAlphabet>)child).Text;
                            break;
                        case KjuAlphabet.TypeIdentifier:
                            type = this.TypeIdentifierAst(child as Token<KjuAlphabet>, diagnostics);
                            break;
                        case KjuAlphabet.Expression:
                            value = this.ExpressionToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            break;
                    }
                }

                return new VariableDeclaration(type, identifier, value);
            }

            private Expression VariableUseToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                string id = ((Token<KjuAlphabet>)branch.Children[0]).Text;
                if (branch.Children.Count == 1)
                {
                    // Value
                    return new Variable(id);
                }
                else
                {
                    // Function call
                    var arguments =
                        this.FunctionCallArgumentsToAst((Brunch<KjuAlphabet>)branch.Children[1], diagnostics);
                    return new FunctionCall(id, arguments);
                }
            }

            private Expression StatementToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var child = branch.Children[0];
                return this.GeneralToAst(child, diagnostics);
            }

            private Expression ExpressionToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                return this.ExpressionAssignmentToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
            }

            private Expression ExpressionAssignmentToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLogicalOrToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operatorSymbol = branch.Children[1].Category;
                    var variable = this.ExpressionLogicalOrToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics) as AST.Variable;
                    var rightValue = this.ExpressionAssignmentToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);

                    if (variable == null)
                    {
                        diagnostics.Add(new Diagnostic(
                            DiagnosticStatus.Error,
                            AssignmentLhsErrorDiagnosticsType,
                            "{0} Left operand of an assignment is not a variable",
                            new List<Range> { branch.Children[0].InputRange }));

                        throw new ParseTreeToAstConverterException(
                            "Left operand of an assignment is not a variable");
                    }

                    if (operatorSymbol == KjuAlphabet.Assign)
                    {
                        return new Assignment(variable, rightValue);
                    }
                    else
                    {
                        var type = this.symbolToOperationType[operatorSymbol];
                        return new CompoundAssignment(variable, type, rightValue);
                    }
                }
            }

            private Expression ExpressionLogicalOrToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var type = LogicalBinaryOperationType.Or;
                    var leftValue =
                        this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                    var rightValue =
                        this.ExpressionLogicalOrToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    return new LogicalBinaryOperation(type, leftValue, rightValue);
                }
            }

            private Expression ExpressionLogicalAndToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionEqualsNotEqualsToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var logicalOperationType = LogicalBinaryOperationType.And;
                    var leftValue = this.ExpressionEqualsNotEqualsToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics);
                    var rightValue =
                        this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    return new LogicalBinaryOperation(logicalOperationType, leftValue, rightValue);
                }
            }

            private Expression ExpressionEqualsNotEqualsToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLessThanGreaterThanToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics);
                }
                else
                {
                    var comparisonType = this.symbolToComparisonType[branch.Children[1].Category];
                    var leftValue = this.ExpressionLessThanGreaterThanToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics);
                    var rightValue =
                        this.ExpressionEqualsNotEqualsToAst(branch.Children[2] as Brunch<KjuAlphabet>, diagnostics);
                    return new Comparison(comparisonType, leftValue, rightValue);
                }
            }

            private Expression ExpressionLessThanGreaterThanToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionPlusMinusToAst(branch.Children[0] as Brunch<KjuAlphabet>, diagnostics);
                }
                else
                {
                    var comparisonType = this.symbolToComparisonType[branch.Children[1].Category];
                    var leftValue = this.ExpressionPlusMinusToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                    var rightValue =
                        this.ExpressionLessThanGreaterThanToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    return new Comparison(comparisonType, leftValue, rightValue);
                }
            }

            private Expression ExpressionPlusMinusToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionTimesDivideModuloToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operationType = this.symbolToOperationType[branch.Children[1].Category];
                    var leftValue = this.ExpressionTimesDivideModuloToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics);
                    var rightValue =
                        this.ExpressionPlusMinusToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    return new ArithmeticOperation(operationType, leftValue, rightValue);
                }
            }

            private Expression ExpressionTimesDivideModuloToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operationType = this.symbolToOperationType[branch.Children[1].Category];
                    var leftValue =
                        this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                    var rightValue =
                        this.ExpressionTimesDivideModuloToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    return new ArithmeticOperation(operationType, leftValue, rightValue);
                }
            }

            private Expression ExpressionLogicalNotToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionAtomToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operationToken = (Token<KjuAlphabet>)branch.Children[0];
                    var operationTokenCategory = operationToken.Category;
                    var operationType = this.symbolToUnaryOperationType[operationTokenCategory];
                    var expression =
                        this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[1], diagnostics);
                    return new UnaryOperation(operationType, expression);
                }
            }

            private Expression ExpressionAtomToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                if (branch.Children.Count == 1)
                {
                    return this.GeneralToAst(branch.Children[0], diagnostics);
                }
                else
                {
                    foreach (var child in branch.Children)
                    {
                        if (child.Category == KjuAlphabet.Statement)
                        {
                            var ast = this.StatementToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            this.enclosedWithParentheses.Add(ast);
                            return ast;
                        }
                    }

                    diagnostics.Add(new Diagnostic(
                        DiagnosticStatus.Error,
                        AstConversionErrorDiagnosticsType,
                        "{0} ExpressionAtom with > 1 child doesn't contain a Statement",
                        new List<Range> { branch.InputRange }));
                    throw new ParseTreeToAstConverterException(
                        "ExpressionAtom with more than 1 child should contain '( statement )'");
                }
            }
        }
    }
}