namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Diagnostics;
    using Lexer;
    using Parser;
    using Types;

    public class KjuParseTreeToAstConverter : IParseTreeToAstConverter<KjuAlphabet>
    {
        public static readonly string TokenCategoryErrorDiagnosticsType = "ToASTConverter.UnexpectedTokenCategory";
        public static readonly string TypeIdentifierErrorDiagnosticsType = "ToASTConverter.UnexpectedTypeIdentifier";
        public static readonly string AstConversionErrorDiagnosticsType = "ToASTConverter.AstConversionError";
        public static readonly string AssignmentLhsErrorDiagnosticsType = "ToASTConverter.AssignmentLhsError";

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
                        [KjuAlphabet.ArrayAlloc] = this.ArrayAllocToAst,
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
                ast.InputRange = parseTree.InputRange;

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
                Expression ret = null;
                switch (token.Category)
                {
                    case KjuAlphabet.Break:
                        ret = new BreakStatement();
                        break;
                    case KjuAlphabet.Continue:
                        ret = new ContinueStatement();
                        break;
                    case KjuAlphabet.DecimalLiteral:
                        var intValue = long.Parse(token.Text);
                        ret = new IntegerLiteral(intValue);
                        break;
                    case KjuAlphabet.BooleanLiteral:
                        var boolValue = bool.Parse(token.Text);
                        ret = new BoolLiteral(boolValue);
                        break;
                    default:
                        var diag = new Diagnostic(
                            DiagnosticStatus.Error,
                            TokenCategoryErrorDiagnosticsType,
                            $"{{0}} Unexpected token category: {token.Category}",
                            new List<Range> { token.InputRange });
                        diagnostics.Add(diag);
                        throw new ParseTreeToAstConverterException("Unexpected category in token");
                }

                ret.InputRange = token.InputRange;
                return ret;
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
                Console.WriteLine(parseTree.Category);
                Console.WriteLine(parseTree);

                if (this.symbolToGenFunction.ContainsKey(parseTree.Category))
                {
                    return this.symbolToGenFunction[parseTree.Category](parseTree as Brunch<KjuAlphabet>, diagnostics);
                }
                else
                {
                    return TokenAst((Token<KjuAlphabet>)parseTree, diagnostics);
                }
            }

            private DataType TypeIdentifierAstToken(Token<KjuAlphabet> token, IDiagnostics diagnostics)
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

            private DataType TypeIdentifierAst(ParseTree<KjuAlphabet> tree, IDiagnostics diagnostics)
            {
                switch (tree)
                {
                    case Token<KjuAlphabet> token:
                        return this.TypeIdentifierAstToken(token, diagnostics);
                    case Brunch<KjuAlphabet> brunch:
                        var parseTreeChild = brunch.Children[1];
                        var childDataType = this.TypeIdentifierAst(parseTreeChild, diagnostics);
                        return ArrayType.GetInstance(childDataType);
                    default:
                        var diag = new Diagnostic(
                            DiagnosticStatus.Error,
                            TypeIdentifierErrorDiagnosticsType,
                            $"Unexpected parse tree type in type identification: '{tree}'",
                            new List<Range>());
                        diagnostics.Add(diag);
                        throw new ParseTreeToAstConverterException("Unexpected parse tree type in type identification");
                }
            }

            private FunctionDeclaration FunctionDeclarationToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var parameters = new List<VariableDeclaration>();
                string identifier = null;
                DataType type = null;
                InstructionBlock body = null;
                bool isForeign = false;
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
                            type = this.TypeIdentifierAst(child, diagnostics);
                            break;
                        case KjuAlphabet.Block:
                            body = this.BlockToAst((Brunch<KjuAlphabet>)child, diagnostics);
                            break;
                        case KjuAlphabet.Import:
                            isForeign = true;
                            break;
                    }
                }

                var ast = new FunctionDeclaration(
                    identifier,
                    type,
                    parameters,
                    body,
                    isForeign);
                ast.InputRange = branch.InputRange;

                return ast;
            }

            private InstructionBlock BlockToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var instructions = branch.Children
                    .Where(child => child.Category == KjuAlphabet.Instruction)
                    .Cast<Brunch<KjuAlphabet>>()
                    .Select(child => this.InstructionToAst(child, diagnostics))
                    .ToList();
                var ret = new InstructionBlock(instructions);
                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression InstructionToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var ret = branch.Children.Count == 1
                    ? new UnitLiteral()
                    : this.NotDelimeteredInstructionToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression NotDelimeteredInstructionToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var ret = this.StatementToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                ret.InputRange = branch.InputRange;
                return ret;
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
                            type = this.TypeIdentifierAst(child, diagnostics);
                            break;
                    }
                }

                var ret = new VariableDeclaration(type, identifier, null);
                ret.InputRange = branch.InputRange;
                return ret;
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

                var ret = new IfStatement(condition, blockList[0], blockList[1]);
                ret.InputRange = branch.InputRange;
                return ret;
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

                var ret = new WhileStatement(condition, body);
                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ArrayAllocToAst(Brunch<KjuAlphabet> brunch, IDiagnostics diagnostics)
            {
                DataType type = null;
                Expression size = null;
                foreach (var child in brunch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.TypeIdentifier:
                            type = this.TypeIdentifierAst(child, diagnostics);
                            break;
                        case KjuAlphabet.Expression:
                            size = this.ExpressionToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            break;
                    }
                }

                return new ArrayAlloc(type, size);
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

                var ret = new ReturnStatement(value);
                ret.InputRange = branch.InputRange;
                return ret;
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
                            type = this.TypeIdentifierAst(child, diagnostics);
                            break;
                        case KjuAlphabet.Expression:
                            value = this.ExpressionToAst(child as Brunch<KjuAlphabet>, diagnostics);
                            break;
                    }
                }

                var ret = new VariableDeclaration(type, identifier, value);
                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression VariableUseToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                string id = ((Token<KjuAlphabet>)branch.Children[0]).Text;
                if (branch.Children.Count == 1)
                {
                    // Value
                    ret = new Variable(id);
                }
                else
                {
                    // Function call
                    var arguments =
                        this.FunctionCallArgumentsToAst((Brunch<KjuAlphabet>)branch.Children[1], diagnostics);
                    ret = new FunctionCall(id, arguments);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression StatementToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var child = branch.Children[0];
                var ret = this.GeneralToAst(child, diagnostics);
                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                var ret = this.ExpressionAssignmentToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionAssignmentToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionLogicalOrToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operatorSymbol = branch.Children[1].Category;
                    var lhs = this.ExpressionLogicalOrToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics);
                    var rightValue =
                        this.ExpressionAssignmentToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);

                    switch (lhs)
                    {
                        case Variable variable:
                            if (operatorSymbol == KjuAlphabet.Assign)
                            {
                                ret = new Assignment(variable, rightValue);
                            }
                            else
                            {
                                var type = this.symbolToOperationType[operatorSymbol];
                                ret = new CompoundAssignment(variable, type, rightValue);
                            }

                            break;

                        case ArrayAccess access:
                            if (operatorSymbol == KjuAlphabet.Assign)
                            {
                                ret = new ArrayAssignment(access, rightValue);
                            }
                            else
                            {
                                var type = this.symbolToOperationType[operatorSymbol];
                                ret = new ArrayCompoundAssignment(access, type, rightValue);
                            }

                            break;

                        default:
                            diagnostics.Add(new Diagnostic(
                                DiagnosticStatus.Error,
                                AssignmentLhsErrorDiagnosticsType,
                                "{0} Left operand of an assignment is not a variable nor array access",
                                new List<Range> { branch.Children[0].InputRange }));

                            throw new ParseTreeToAstConverterException(
                                "Left operand of an assignment is not a variable nor array access");
                    }
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionLogicalOrToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var type = LogicalBinaryOperationType.Or;
                    var leftValue =
                        this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                    var rightValue =
                        this.ExpressionLogicalOrToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    ret = new LogicalBinaryOperation(type, leftValue, rightValue);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionLogicalAndToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionEqualsNotEqualsToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var logicalOperationType = LogicalBinaryOperationType.And;
                    var leftValue = this.ExpressionEqualsNotEqualsToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics);
                    var rightValue =
                        this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    ret = new LogicalBinaryOperation(logicalOperationType, leftValue, rightValue);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionEqualsNotEqualsToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionLessThanGreaterThanToAst(
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
                    ret = new Comparison(comparisonType, leftValue, rightValue);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionLessThanGreaterThanToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionPlusMinusToAst(branch.Children[0] as Brunch<KjuAlphabet>, diagnostics);
                }
                else
                {
                    var comparisonType = this.symbolToComparisonType[branch.Children[1].Category];
                    var leftValue = this.ExpressionPlusMinusToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                    var rightValue =
                        this.ExpressionLessThanGreaterThanToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    ret = new Comparison(comparisonType, leftValue, rightValue);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionPlusMinusToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionTimesDivideModuloToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operationType = this.symbolToOperationType[branch.Children[1].Category];
                    var leftValue = this.ExpressionTimesDivideModuloToAst(
                        (Brunch<KjuAlphabet>)branch.Children[0],
                        diagnostics);
                    var rightValue =
                        this.ExpressionPlusMinusToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    ret = new ArithmeticOperation(operationType, leftValue, rightValue);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionTimesDivideModuloToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operationType = this.symbolToOperationType[branch.Children[1].Category];
                    var leftValue =
                        this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                    var rightValue =
                        this.ExpressionTimesDivideModuloToAst((Brunch<KjuAlphabet>)branch.Children[2], diagnostics);
                    ret = new ArithmeticOperation(operationType, leftValue, rightValue);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionLogicalNotToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                Expression ret = null;
                if (branch.Children.Count == 1)
                {
                    ret = this.ExpressionAtomToAst((Brunch<KjuAlphabet>)branch.Children[0], diagnostics);
                }
                else
                {
                    var operationToken = (Token<KjuAlphabet>)branch.Children[0];
                    var operationTokenCategory = operationToken.Category;
                    var operationType = this.symbolToUnaryOperationType[operationTokenCategory];
                    var expression =
                        this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[1], diagnostics);
                    ret = new UnaryOperation(operationType, expression);
                }

                ret.InputRange = branch.InputRange;
                return ret;
            }

            private Expression ExpressionAtomToAst(Brunch<KjuAlphabet> branch, IDiagnostics diagnostics)
            {
                // Literal
                if (branch.Children.Count == 1)
                {
                    var ret = this.GeneralToAst(branch.Children[0], diagnostics);
                    ret.InputRange = branch.InputRange;
                    return ret;
                }

                // (Statement)
                foreach (var child in branch.Children)
                {
                    if (child.Category == KjuAlphabet.Statement)
                    {
                        var ast = this.StatementToAst(child as Brunch<KjuAlphabet>, diagnostics);
                        this.enclosedWithParentheses.Add(ast);
                        return ast;
                    }
                }

                Expression primaryExpression = null;
                var firstChild = branch.Children[0];
                switch (firstChild.Category)
                {
                    case KjuAlphabet.VariableUse:
                        primaryExpression = this.VariableUseToAst(firstChild as Brunch<KjuAlphabet>, diagnostics);
                        break;
                    case KjuAlphabet.ArrayAlloc:
                        primaryExpression = this.ArrayAllocToAst(firstChild as Brunch<KjuAlphabet>, diagnostics);
                        break;
                    default:
                        const string message = "ExpressionAtom with > 1 children should contain Statement of be either VarUse or ArrayAlloc";
                        diagnostics.Add(new Diagnostic(
                            DiagnosticStatus.Error,
                            AstConversionErrorDiagnosticsType,
                            $"{{0}} {message}",
                            new List<Range> { branch.InputRange }));
                        throw new ParseTreeToAstConverterException(
                            message);
                }

                Console.WriteLine(branch);
                return branch
                    .Children
                    .Skip(1)
                    .Select(parseTree =>
                    {
                        var mainBranch = parseTree as Brunch<KjuAlphabet>;
                        var expressionBranch = mainBranch.Children[1] as Brunch<KjuAlphabet>;
                        return this.ExpressionToAst(expressionBranch, diagnostics);
                    })
                    .Aggregate(primaryExpression, (agg, expr) => new ArrayAccess(agg, expr));
            }
        }
    }
}