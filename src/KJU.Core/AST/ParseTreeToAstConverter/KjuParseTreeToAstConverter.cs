namespace KJU.Core.AST.ParseTreeToAstConverter
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
                    this.symbolToUnaryOperationType,
                    diagnostics)
                .GenerateAst(parseTree);
        }

        private class ConverterProcess
        {
            private readonly Dictionary<KjuAlphabet, ArithmeticOperationType> symbolToOperationType;
            private readonly Dictionary<KjuAlphabet, ComparisonType> symbolToComparisonType;
            private readonly Dictionary<KjuAlphabet, UnaryOperationType> symbolToUnaryOperationType;
            private readonly IDiagnostics diagnostics;
            private readonly Dictionary<string, List<KeyValuePair<StructDeclaration, StructField>>> fieldsToStructDeclarations = new Dictionary<string, List<KeyValuePair<StructDeclaration, StructField>>>();

            private readonly Dictionary<KjuAlphabet, Func<Brunch<KjuAlphabet>, Expression>>
                symbolToGenFunction;

            private readonly IOperationOrderFlipper operationOrderFlipper = new OperationOrderFlipper();

            public ConverterProcess(
                Dictionary<KjuAlphabet, ArithmeticOperationType> symbolToOperationType,
                Dictionary<KjuAlphabet, ComparisonType> symbolToComparisonType,
                Dictionary<KjuAlphabet, UnaryOperationType> symbolToUnaryOperationType,
                IDiagnostics diagnostics)
            {
                this.symbolToOperationType = symbolToOperationType;
                this.symbolToComparisonType = symbolToComparisonType;
                this.symbolToUnaryOperationType = symbolToUnaryOperationType;
                this.diagnostics = diagnostics;
                this.symbolToGenFunction =
                    new Dictionary<KjuAlphabet, Func<Brunch<KjuAlphabet>, Expression>>()
                    {
                        [KjuAlphabet.FunctionDefinition] = this.FunctionDeclarationToAst,
                        [KjuAlphabet.Block] = this.BlockToAst,
                        [KjuAlphabet.Instruction] = this.InstructionToAst,
                        [KjuAlphabet.NotDelimeteredInstruction] = this.NotDelimeteredInstructionToAst,
                        [KjuAlphabet.FunctionParameter] = this.FunctionParameterToAst,
                        [KjuAlphabet.IfStatement] = this.IfStatementToAst,
                        [KjuAlphabet.WhileStatement] = this.WhileStatementToAst,
                        [KjuAlphabet.Alloc] = this.AllocToAst,
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
                        [KjuAlphabet.StructDefinition] = this.StructDefinitionAst,
                        [KjuAlphabet.Literal] = this.LiteralToAst,
                        [KjuAlphabet.UnapplyExpression] = this.UnapplyExpressionAst,
                        [KjuAlphabet.ApplyExpression] = this.ApplyExpressionAst,
                    };
            }

            public Node GenerateAst(ParseTree<KjuAlphabet> parseTree)
            {
                var branch = (Brunch<KjuAlphabet>)parseTree;
                var globals = branch.Children
                    .Select(this.GeneralToAst)
                    .ToList();
                var structs = globals.OfType<StructDeclaration>().ToList();
                var functions = globals.OfType<FunctionDeclaration>().ToList();

                var ast = new Program(parseTree.InputRange, structs, functions);
                this.operationOrderFlipper.FlipToLeftAssignmentAst(ast);
                return ast;
            }

            private Expression TokenToAst(Token<KjuAlphabet> token)
            {
                switch (token.Category)
                {
                    case KjuAlphabet.Break:
                        return new BreakStatement(token.InputRange);
                    case KjuAlphabet.Continue:
                        return new ContinueStatement(token.InputRange);
                    case KjuAlphabet.DecimalLiteral:
                        var intValue = long.Parse(token.Text);
                        return new IntegerLiteral(token.InputRange, intValue);
                    case KjuAlphabet.BooleanLiteral:
                        var boolValue = bool.Parse(token.Text);
                        return new BoolLiteral(token.InputRange, boolValue);
                    case KjuAlphabet.NullLiteral:
                        return new NullLiteral(token.InputRange);
                    default:
                        var diag = new Diagnostic(
                            DiagnosticStatus.Error,
                            TokenCategoryErrorDiagnosticsType,
                            $"Unexpected token category: {token.Category}",
                            new List<Range> { token.InputRange });
                        this.diagnostics.Add(diag);
                        throw new ParseTreeToAstConverterException("Unexpected category in token");
                }
            }

            private Expression GeneralToAst(ParseTree<KjuAlphabet> parseTree)
            {
                switch (parseTree)
                {
                    case Brunch<KjuAlphabet> brunch:
                        return this.symbolToGenFunction[parseTree.Category](brunch);
                    case Token<KjuAlphabet> token:
                        return this.TokenToAst(token);
                    default:
                        var diag = new Diagnostic(
                            DiagnosticStatus.Error,
                            TypeIdentifierErrorDiagnosticsType,
                            $"Unexpected ParseTree type {parseTree.GetType()}'",
                            new List<Range>());
                        this.diagnostics.Add(diag);
                        throw new ParseTreeToAstConverterException($"Unexpected ParseTree type {parseTree.GetType()}");
                }
            }

            private DataType FunctionTypeIdentifierAst(Brunch<KjuAlphabet> brunch)
            {
                var argumentsTypes = brunch.Children
                    .Where(child => child.Category == KjuAlphabet.FunctionTypeArgumentType)
                    .Select(this.TypeIdentifierAst);

                var returnType = brunch.Children
                    .Where(child => child.Category == KjuAlphabet.FunctionTypeResultType)
                    .Select(this.TypeIdentifierAst)
                    .Single();
                return new UnresolvedFunctionType(argumentsTypes, returnType);
            }

            private DataType ArrayTypeIdentifierAst(Brunch<KjuAlphabet> brunch)
            {
                var childDataType = this.TypeIdentifierAst(brunch.Children[1]);
                return new UnresolvedArrayType(childDataType);
            }

            private DataType TypeIdentifierAst(ParseTree<KjuAlphabet> tree)
            {
                switch (tree)
                {
                    case Token<KjuAlphabet> token:
                        return new UnresolvedType(token.Text, token.InputRange);
                    case Brunch<KjuAlphabet> brunch:
                        Console.WriteLine(brunch);
                        var child = brunch.Children[0];
                        switch (child.Category)
                        {
                            case KjuAlphabet.TypeIdentifier:
                                return this.TypeIdentifierAst(brunch.Children[0]);
                            case KjuAlphabet.FunctionTypeArgumentType:
                                return this.TypeIdentifierAst(brunch.Children[0]);
                            case KjuAlphabet.FunctionTypeResultType:
                                return this.TypeIdentifierAst(brunch.Children[0]);
                            case KjuAlphabet.FunctionTypeIdentifier:
                                return this.FunctionTypeIdentifierAst((Brunch<KjuAlphabet>)brunch.Children[0]);
                            case KjuAlphabet.ArrayTypeIdentifier:
                                return this.ArrayTypeIdentifierAst((Brunch<KjuAlphabet>)brunch.Children[0]);
                            case KjuAlphabet.UnknownTypeIdentifier:
                                return null;
                            default:
                                throw new ParseTreeToAstConverterException($"Unknown type category: {child.Category}");
                        }

                    default:
                        var diag = new Diagnostic(
                            DiagnosticStatus.Error,
                            TypeIdentifierErrorDiagnosticsType,
                            $"Unexpected parse tree type in type identification: '{tree}'",
                            new List<Range>());
                        this.diagnostics.Add(diag);
                        throw new ParseTreeToAstConverterException("Unexpected parse tree type in type identification");
                }
            }

            private Expression UnapplyExpressionAst(Brunch<KjuAlphabet> brunch)
            {
                var nameToken = (Token<KjuAlphabet>)brunch.Children[2];
                var nameText = nameToken.Text;
                return new UnApplication(brunch.InputRange, nameText);
            }

            private Expression ApplyExpressionAst(Brunch<KjuAlphabet> brunch)
            {
                var function = this.GeneralToAst(brunch.Children[2]);
                var arguments =
                    brunch
                        .Children
                        .Skip(3)
                        .Where(x => x.Category == KjuAlphabet.Expression)
                        .Select(this.GeneralToAst).ToList();
                return new Application(brunch.InputRange, function, arguments);
            }

            private FunctionDeclaration FunctionDeclarationToAst(Brunch<KjuAlphabet> branch)
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
                            parameters.Add(this.FunctionParameterToAst((Brunch<KjuAlphabet>)child));
                            break;
                        case KjuAlphabet.VariableFunctionIdentifier:
                            identifier = ((Token<KjuAlphabet>)child).Text;
                            break;
                        case KjuAlphabet.TypeDeclaration:
                            type = this.TypeDeclarationAst((Brunch<KjuAlphabet>)child);
                            break;
                        case KjuAlphabet.Block:
                            body = this.BlockToAst((Brunch<KjuAlphabet>)child);
                            break;
                        case KjuAlphabet.Import:
                            isForeign = true;
                            break;
                    }
                }

                return new FunctionDeclaration(
                    branch.InputRange,
                    identifier,
                    type,
                    parameters,
                    body,
                    isForeign);
            }

            private InstructionBlock BlockToAst(Brunch<KjuAlphabet> branch)
            {
                var instructions = branch.Children
                    .Where(child => child.Category == KjuAlphabet.Instruction)
                    .Cast<Brunch<KjuAlphabet>>()
                    .Select(this.InstructionToAst)
                    .ToList();
                return new InstructionBlock(branch.InputRange, instructions);
            }

            private Expression InstructionToAst(Brunch<KjuAlphabet> branch)
            {
                return branch.Children.Count == 1
                    ? new UnitLiteral(branch.InputRange)
                    : this.NotDelimeteredInstructionToAst((Brunch<KjuAlphabet>)branch.Children[0]);
            }

            private Expression NotDelimeteredInstructionToAst(Brunch<KjuAlphabet> branch)
            {
                return this.StatementToAst((Brunch<KjuAlphabet>)branch.Children[0]);
            }

            private DataType TypeDeclarationAst(Brunch<KjuAlphabet> brunch)
            {
                return this.TypeIdentifierAst(brunch.Children[1]);
            }

            private VariableDeclaration FunctionParameterToAst(Brunch<KjuAlphabet> branch)
            {
                var identifier = ((Token<KjuAlphabet>)branch.Children[0]).Text;
                var type = branch.Children.Count == 2
                    ? this.TypeDeclarationAst((Brunch<KjuAlphabet>)branch.Children[1])
                    : null;
                return new VariableDeclaration(branch.InputRange, type, identifier, null);
            }

            private List<Expression> FunctionCallArgumentsToAst(Brunch<KjuAlphabet> branch)
            {
                return branch.Children
                    .Where(child => child.Category == KjuAlphabet.Expression)
                    .Cast<Brunch<KjuAlphabet>>()
                    .Select(this.ExpressionToAst)
                    .ToList();
            }

            private IfStatement IfStatementToAst(Brunch<KjuAlphabet> branch)
            {
                var blockList = new List<InstructionBlock>();
                Expression condition = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.Expression:
                            condition = this.ExpressionToAst(child as Brunch<KjuAlphabet>);
                            break;
                        case KjuAlphabet.Block:
                            blockList.Add(this.BlockToAst(child as Brunch<KjuAlphabet>));
                            break;
                    }
                }

                var elseBody = blockList.Count == 1
                    ? new InstructionBlock(branch.InputRange, new List<Expression>())
                    : blockList[1];

                var ret = new IfStatement(
                    branch.InputRange,
                    condition,
                    blockList[0],
                    elseBody);
                return ret;
            }

            private WhileStatement WhileStatementToAst(Brunch<KjuAlphabet> branch)
            {
                var condition = this.ExpressionToAst((Brunch<KjuAlphabet>)branch.Children[1]);
                var body = this.BlockToAst((Brunch<KjuAlphabet>)branch.Children[2]);
                return new WhileStatement(branch.InputRange, condition, body);
            }

            private Expression AllocToAst(Brunch<KjuAlphabet> brunch)
            {
                var type = this.TypeIdentifierAst(brunch.Children[2]);
                switch (brunch.Children.Count)
                {
                    case 4:
                    {
                        return new StructAlloc(brunch.InputRange, type);
                    }

                    case 6:
                    {
                        var size = this.ExpressionToAst((Brunch<KjuAlphabet>)brunch.Children[4]);
                        return new ArrayAlloc(brunch.InputRange, type, size);
                    }

                    default:
                    {
                        this.diagnostics.Add(
                            new Diagnostic(
                                DiagnosticStatus.Error,
                                AssignmentLhsErrorDiagnosticsType,
                                $"Unexpected number of children: {brunch.Children.Count}",
                                new List<Range> { brunch.InputRange }));

                        throw new ParseTreeToAstConverterException(
                            $"Unexpected number of children: {brunch.Children.Count} at {brunch.InputRange}");
                    }
                }
            }

            private Expression ReturnStatementToAst(Brunch<KjuAlphabet> branch)
            {
                Expression value = null;
                foreach (var child in branch.Children)
                {
                    switch (child.Category)
                    {
                        case KjuAlphabet.Expression:
                            value = this.ExpressionToAst(child as Brunch<KjuAlphabet>);
                            break;
                    }
                }

                return new ReturnStatement(branch.InputRange, value);
            }

            private VariableDeclaration VariableDeclarationToAst(Brunch<KjuAlphabet> branch)
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
                        case KjuAlphabet.TypeDeclaration:
                            type = this.TypeDeclarationAst((Brunch<KjuAlphabet>)child);
                            break;
                        case KjuAlphabet.Expression:
                            value = this.ExpressionToAst(child as Brunch<KjuAlphabet>);
                            break;
                    }
                }

                return new VariableDeclaration(branch.InputRange, type, identifier, value);
            }

            private Expression VariableUseToAst(Brunch<KjuAlphabet> branch)
            {
                string id = ((Token<KjuAlphabet>)branch.Children[0]).Text;
                if (branch.Children.Count == 1)
                {
                    // Value
                    return new Variable(branch.InputRange, id);
                }
                else
                {
                    // Function call
                    var arguments =
                        this.FunctionCallArgumentsToAst((Brunch<KjuAlphabet>)branch.Children[1]);
                    return new FunctionCall(branch.InputRange, id, arguments);
                }
            }

            private Expression StatementToAst(Brunch<KjuAlphabet> branch)
            {
                return this.GeneralToAst(branch.Children[0]);
            }

            private Expression ExpressionToAst(Brunch<KjuAlphabet> branch)
            {
                return this.ExpressionAssignmentToAst((Brunch<KjuAlphabet>)branch.Children[0]);
            }

            private Expression ExpressionAssignmentToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLogicalOrToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                }
                else
                {
                    var operatorSymbol = branch.Children[1].Category;
                    var lhs = this.ExpressionLogicalOrToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                    var rightValue =
                        this.ExpressionAssignmentToAst((Brunch<KjuAlphabet>)branch.Children[2]);

                    switch (lhs)
                    {
                        case Variable variable:
                            if (operatorSymbol == KjuAlphabet.Assign)
                            {
                                return new Assignment(branch.InputRange, variable, rightValue);
                            }
                            else
                            {
                                var type = this.symbolToOperationType[operatorSymbol];
                                return new CompoundAssignment(branch.InputRange, variable, type, rightValue);
                            }

                        case ArrayAccess arrayAccess:
                            if (operatorSymbol == KjuAlphabet.Assign)
                            {
                                return new ComplexAssignment(branch.InputRange, arrayAccess, rightValue);
                            }
                            else
                            {
                                var type = this.symbolToOperationType[operatorSymbol];
                                return new ComplexCompoundAssignment(branch.InputRange, arrayAccess, type, rightValue);
                            }

                        case FieldAccess fieldAccess:

                            if (operatorSymbol == KjuAlphabet.Assign)
                            {
                                return new ComplexAssignment(branch.InputRange, fieldAccess, rightValue);
                            }
                            else
                            {
                                var type = this.symbolToOperationType[operatorSymbol];
                                return new ComplexCompoundAssignment(branch.InputRange, fieldAccess, type, rightValue);
                            }

                        default:
                            this.diagnostics.Add(
                                new Diagnostic(
                                    DiagnosticStatus.Error,
                                    AssignmentLhsErrorDiagnosticsType,
                                    $"Left operand of an assignment is not a variable nor array access but {lhs}",
                                    new List<Range> { branch.Children[0].InputRange }));

                            throw new ParseTreeToAstConverterException(
                                $"Left operand of an assignment is not a variable nor array access but {lhs}");
                    }
                }
            }

            private Expression ExpressionLogicalOrToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                }
                else
                {
                    var type = LogicalBinaryOperationType.Or;
                    var leftValue =
                        this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                    var rightValue =
                        this.ExpressionLogicalOrToAst((Brunch<KjuAlphabet>)branch.Children[2]);
                    return new LogicalBinaryOperation(branch.InputRange, leftValue, rightValue, type);
                }
            }

            private Expression ExpressionLogicalAndToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionEqualsNotEqualsToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                }
                else
                {
                    var logicalOperationType = LogicalBinaryOperationType.And;
                    var leftValue = this.ExpressionEqualsNotEqualsToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                    var rightValue =
                        this.ExpressionLogicalAndToAst((Brunch<KjuAlphabet>)branch.Children[2]);
                    return new LogicalBinaryOperation(branch.InputRange, leftValue, rightValue, logicalOperationType);
                }
            }

            private Expression ExpressionEqualsNotEqualsToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLessThanGreaterThanToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                }
                else
                {
                    var comparisonType = this.symbolToComparisonType[branch.Children[1].Category];
                    var leftValue = this.ExpressionLessThanGreaterThanToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                    var rightValue =
                        this.ExpressionEqualsNotEqualsToAst(branch.Children[2] as Brunch<KjuAlphabet>);
                    return new Comparison(branch.InputRange, leftValue, rightValue, comparisonType);
                }
            }

            private Expression ExpressionLessThanGreaterThanToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionPlusMinusToAst(branch.Children[0] as Brunch<KjuAlphabet>);
                }
                else
                {
                    var comparisonType = this.symbolToComparisonType[branch.Children[1].Category];
                    var leftValue = this.ExpressionPlusMinusToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                    var rightValue =
                        this.ExpressionLessThanGreaterThanToAst((Brunch<KjuAlphabet>)branch.Children[2]);
                    return new Comparison(branch.InputRange, leftValue, rightValue, comparisonType);
                }
            }

            private Expression ExpressionPlusMinusToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionTimesDivideModuloToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                }
                else
                {
                    var operationType = this.symbolToOperationType[branch.Children[1].Category];
                    var leftValue = this.ExpressionTimesDivideModuloToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                    var rightValue =
                        this.ExpressionPlusMinusToAst((Brunch<KjuAlphabet>)branch.Children[2]);
                    return new ArithmeticOperation(branch.InputRange, leftValue, rightValue, operationType);
                }
            }

            private Expression ExpressionTimesDivideModuloToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                }
                else
                {
                    var operationType = this.symbolToOperationType[branch.Children[1].Category];
                    var leftValue =
                        this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                    var rightValue =
                        this.ExpressionTimesDivideModuloToAst((Brunch<KjuAlphabet>)branch.Children[2]);
                    return new ArithmeticOperation(branch.InputRange, leftValue, rightValue, operationType);
                }
            }

            private Expression AccessToAst(Brunch<KjuAlphabet> branch)
            {
                var atom = branch.Children.First();
                var accesses = branch.Children.Skip(1).Cast<Brunch<KjuAlphabet>>();
                return accesses.Aggregate(
                    this.ExpressionAtomToAst((Brunch<KjuAlphabet>)atom),
                    (expression, access) =>
                    {
                        var accessType = (Brunch<KjuAlphabet>)access.Children[0];
                        switch (accessType.Category)
                        {
                            case KjuAlphabet.ArrayAccess:
                            {
                                return new ArrayAccess(
                                    branch.InputRange,
                                    expression,
                                    this.GeneralToAst(accessType.Children[1]));
                            }

                            case KjuAlphabet.FieldAccess:
                            {
                                var labelToken = (Token<KjuAlphabet>)accessType.Children[1];
                                var label = labelToken.Text;
                                    // Warning: the below list gets modified later by StructDeclarations.
                                    List<KeyValuePair<StructDeclaration, StructField>> candidates;
                                try
                                {
                                    candidates = this.fieldsToStructDeclarations[label];
                                }
                                catch (KeyNotFoundException)
                                {
                                    candidates = new List<KeyValuePair<StructDeclaration, StructField>>();
                                    this.fieldsToStructDeclarations.Add(label, candidates);
                                }
                                var ret = new FieldAccess(branch.InputRange, expression, label);
                                ret.StructCandidates = candidates;
                                return ret;
                            }

                            default:
                                var message = $"Unknown access type: {accessType.Category}";
                                this.diagnostics.Add(
                                    new Diagnostic(
                                        DiagnosticStatus.Error,
                                        AstConversionErrorDiagnosticsType,
                                        message,
                                        new List<Range> { branch.InputRange }));
                                throw new ParseTreeToAstConverterException(
                                    message);
                        }
                    });
            }

            private Expression ExpressionLogicalNotToAst(Brunch<KjuAlphabet> branch)
            {
                if (branch.Children.Count == 1)
                {
                    return this.AccessToAst((Brunch<KjuAlphabet>)branch.Children[0]);
                }

                var operationToken = (Token<KjuAlphabet>)branch.Children[0];
                var operationTokenCategory = operationToken.Category;
                var operationType = this.symbolToUnaryOperationType[operationTokenCategory];
                var expression =
                    this.ExpressionLogicalNotToAst((Brunch<KjuAlphabet>)branch.Children[1]);
                return new UnaryOperation(branch.InputRange, operationType, expression);
            }

            private Expression ParenEnclosedStatementToAst(Brunch<KjuAlphabet> branch)
            {
                var result = this.StatementToAst((Brunch<KjuAlphabet>)branch.Children[1]);
                this.operationOrderFlipper.AddEnclosedWithParentheses(result);
                return result;
            }

            private Expression LiteralToAst(Brunch<KjuAlphabet> branch)
            {
                return this.GeneralToAst(branch.Children[0]);
            }

            private Expression ExpressionAtomToAst(Brunch<KjuAlphabet> branch)
            {
                var firstChild = branch.Children[0];
                Expression primaryExpression;

                switch (firstChild.Category)
                {
                    case KjuAlphabet.Literal:
                    {
                        primaryExpression = this.GeneralToAst(firstChild);
                        break;
                    }

                    case KjuAlphabet.VariableUse:
                    {
                        primaryExpression = this.VariableUseToAst((Brunch<KjuAlphabet>)firstChild);
                        break;
                    }

                    case KjuAlphabet.Alloc:
                    {
                        primaryExpression = this.AllocToAst((Brunch<KjuAlphabet>)firstChild);
                        break;
                    }

                    case KjuAlphabet.ParenEnclosedStatement:
                    {
                        primaryExpression =
                            this.ParenEnclosedStatementToAst((Brunch<KjuAlphabet>)firstChild);
                        break;
                    }

                    default:
                        var message = $"Unknown expression atom: {firstChild.Category}";
                        this.diagnostics.Add(
                            new Diagnostic(
                                DiagnosticStatus.Error,
                                AstConversionErrorDiagnosticsType,
                                message,
                                new List<Range> { branch.InputRange }));
                        throw new ParseTreeToAstConverterException(message);
                }

                return primaryExpression;
            }

            private StructField StructFieldAst(Brunch<KjuAlphabet> branch)
            {
                var nameToken = (Token<KjuAlphabet>)branch.Children[0];
                var name = nameToken.Text;
                var typeIdentifier = branch.Children.Count == 3
                    ? this.TypeDeclarationAst((Brunch<KjuAlphabet>)branch.Children[1])
                    : null;

                return new StructField(branch.InputRange, name, typeIdentifier);
            }

            private List<StructField> StructFieldsAstList(Brunch<KjuAlphabet> branch)
            {
                return branch.Children.Cast<Brunch<KjuAlphabet>>().Select(this.StructFieldAst).ToList();
            }

            private StructDeclaration StructDefinitionAst(Brunch<KjuAlphabet> branch)
            {
                var nameToken = (Token<KjuAlphabet>)branch.Children[1];
                var name = nameToken.Text;
                var fieldsBranch = (Brunch<KjuAlphabet>)branch.Children[3];
                var fieldsNodes = this.StructFieldsAstList(fieldsBranch);
                var ret = new StructDeclaration(branch.InputRange, name, fieldsNodes);
                foreach (StructField field in fieldsNodes)
                {
                    List<KeyValuePair<StructDeclaration, StructField>> candidates;
                    try
                    {
                        candidates = this.fieldsToStructDeclarations[field.Name];
                    }
                    catch (KeyNotFoundException)
                    {
                        candidates = new List<KeyValuePair<StructDeclaration, StructField>>();
                        this.fieldsToStructDeclarations.Add(field.Name, candidates);
                    }

                    candidates.Add(new KeyValuePair<StructDeclaration, StructField>(ret, field));
                }

                return ret;
            }
        }
    }
}