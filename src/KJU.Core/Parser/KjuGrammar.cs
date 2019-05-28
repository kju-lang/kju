namespace KJU.Core.Parser
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Lexer;
    using Regex;
    using static Regex.RegexUtils;

    public static class KjuGrammar
    {
        public static readonly Rule<KjuAlphabet> Kju = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Kju,
            Rhs = Sum(
                KjuAlphabet.FunctionDefinition.ToRegex(),
                KjuAlphabet.StructDefinition.ToRegex()).Starred()
        };

        public static readonly Rule<KjuAlphabet> FunctionDefinition = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.FunctionDefinition,
            Rhs = Concat(
                KjuAlphabet.Fun.ToRegex(),
                KjuAlphabet.VariableFunctionIdentifier.ToRegex(),
                KjuAlphabet.LParen.ToRegex(),
                KjuAlphabet.FunctionParameter.ToRegex().SeparatedBy(KjuAlphabet.Comma.ToRegex()),
                KjuAlphabet.RParen.ToRegex(),
                KjuAlphabet.Colon.ToRegex(),
                KjuAlphabet.TypeIdentifier.ToRegex(),
                Sum(
                    KjuAlphabet.Block.ToRegex(),
                    KjuAlphabet.Import.ToRegex()))
        };

        public static readonly Rule<KjuAlphabet> Block = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Block,
            Rhs = Concat(
                KjuAlphabet.LBrace.ToRegex(),
                KjuAlphabet.Instruction.ToRegex().Starred(),
                KjuAlphabet.RBrace.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Instruction = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Instruction,
            Rhs = Concat(
                KjuAlphabet.NotDelimeteredInstruction.ToRegex().Optional(),
                KjuAlphabet.Semicolon.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> NotDelimeteredInstruction = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.NotDelimeteredInstruction,
            Rhs = KjuAlphabet.Statement.ToRegex()
        };

        public static readonly Rule<KjuAlphabet> FunctionParameter = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.FunctionParameter,
            Rhs = Concat(
                KjuAlphabet.VariableFunctionIdentifier.ToRegex(),
                KjuAlphabet.Colon.ToRegex(),
                KjuAlphabet.TypeIdentifier.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> FunctionCall = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.FunctionCall,
            Rhs = Concat(
                KjuAlphabet.LParen.ToRegex(),
                KjuAlphabet.Expression.ToRegex().SeparatedBy(KjuAlphabet.Comma.ToRegex()),
                KjuAlphabet.RParen.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> TypeIdentifier = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.TypeIdentifier,
            Rhs = Sum(
                Concat(
                    KjuAlphabet.LBracket.ToRegex(),
                    KjuAlphabet.TypeIdentifier.ToRegex(),
                    KjuAlphabet.RBracket.ToRegex()),
                Concat(
                    KjuAlphabet.LParen.ToRegex(),
                    KjuAlphabet.TypeIdentifier.ToRegex().SeparatedBy(KjuAlphabet.Comma.ToRegex()),
                    KjuAlphabet.RParen.ToRegex(),
                    KjuAlphabet.Arrow.ToRegex(),
                    KjuAlphabet.TypeIdentifier.ToRegex()))
        };

        public static readonly Rule<KjuAlphabet> IfStatement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.IfStatement,
            Rhs = Concat(
                KjuAlphabet.If.ToRegex(),
                KjuAlphabet.Expression.ToRegex(),
                KjuAlphabet.Then.ToRegex(),
                KjuAlphabet.Block.ToRegex(),
                Concat(
                    KjuAlphabet.Else.ToRegex(),
                    KjuAlphabet.Block.ToRegex()).Optional())
        };

        public static readonly Rule<KjuAlphabet> WhileStatement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.WhileStatement,
            Rhs = Concat(
                KjuAlphabet.While.ToRegex(),
                KjuAlphabet.Expression.ToRegex(),
                KjuAlphabet.Block.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> ReturnStatement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ReturnStatement,
            Rhs = Concat(
                KjuAlphabet.Return.ToRegex(),
                KjuAlphabet.Expression.ToRegex().Optional())
        };

        public static readonly Rule<KjuAlphabet> VariableDeclaration = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.VariableDeclaration,
            Rhs = Concat(
                KjuAlphabet.Var.ToRegex(),
                KjuAlphabet.VariableFunctionIdentifier.ToRegex(),
                KjuAlphabet.Colon.ToRegex(),
                KjuAlphabet.TypeIdentifier.ToRegex(),
                Concat(
                    KjuAlphabet.Assign.ToRegex(),
                    KjuAlphabet.Expression.ToRegex()).Optional())
        };

        public static readonly Rule<KjuAlphabet> VariableUse = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.VariableUse,
            Rhs = Concat(
                KjuAlphabet.VariableFunctionIdentifier.ToRegex(),
                Sum(
                    KjuAlphabet.FunctionCall.ToRegex(), // Function call
                    new EpsilonRegex<KjuAlphabet>())) // Value read
        };

        public static readonly Rule<KjuAlphabet> Statement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Statement,

            Rhs = Sum(
                KjuAlphabet.VariableDeclaration.ToRegex(),
                KjuAlphabet.ReturnStatement.ToRegex(),
                KjuAlphabet.IfStatement.ToRegex(),
                KjuAlphabet.WhileStatement.ToRegex(),
                KjuAlphabet.Block.ToRegex(),
                KjuAlphabet.Break.ToRegex(),
                KjuAlphabet.Continue.ToRegex(),
                KjuAlphabet.FunctionDefinition.ToRegex(),
                KjuAlphabet.StructDefinition.ToRegex(),
                KjuAlphabet.Expression.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Expression = new Rule<KjuAlphabet>
        {
            Name = "Expression",
            Lhs = KjuAlphabet.Expression,
            Rhs = KjuAlphabet.ExpressionAssignment.ToRegex()
        };

        public static readonly Rule<KjuAlphabet> ExpressionAssignment =
            CreateExpressionRule(
                KjuAlphabet.ExpressionAssignment,
                KjuAlphabet.ExpressionOr,
                KjuAlphabet.Assign,
                KjuAlphabet.PlusAssign,
                KjuAlphabet.MinusAssign,
                KjuAlphabet.StarAssign,
                KjuAlphabet.SlashAssign,
                KjuAlphabet.PercentAssign);

        public static readonly Rule<KjuAlphabet> ExpressionLogicalOr =
            CreateExpressionRule(
                KjuAlphabet.ExpressionOr,
                KjuAlphabet.ExpressionAnd,
                KjuAlphabet.LogicalOr);

        public static readonly Rule<KjuAlphabet> ExpressionLogicalAnd =
            CreateExpressionRule(
                KjuAlphabet.ExpressionAnd,
                KjuAlphabet.ExpressionEqualsNotEquals,
                KjuAlphabet.LogicalAnd);

        public static readonly Rule<KjuAlphabet> ExpressionEqualsNotEquals =
            CreateExpressionRule(
                KjuAlphabet.ExpressionEqualsNotEquals,
                KjuAlphabet.ExpressionLessThanGreaterThan,
                KjuAlphabet.Equals,
                KjuAlphabet.NotEquals);

        public static readonly Rule<KjuAlphabet> ExpressionLessThanGreaterThan = CreateExpressionRule(
            KjuAlphabet.ExpressionLessThanGreaterThan,
            KjuAlphabet.ExpressionPlusMinus,
            KjuAlphabet.LessThan,
            KjuAlphabet.LessOrEqual,
            KjuAlphabet.GreaterThan,
            KjuAlphabet.GreaterOrEqual);

        public static readonly Rule<KjuAlphabet> ExpressionPlusMinus = CreateExpressionRule(
            KjuAlphabet.ExpressionPlusMinus,
            KjuAlphabet.ExpressionTimesDivideModulo,
            KjuAlphabet.Plus,
            KjuAlphabet.Minus);

        public static readonly Rule<KjuAlphabet> ExpressionTimesDivideModulo = CreateExpressionRule(
            KjuAlphabet.ExpressionTimesDivideModulo,
            KjuAlphabet.ExpressionUnaryOperator,
            KjuAlphabet.Star,
            KjuAlphabet.Slash,
            KjuAlphabet.Percent);

        public static readonly Rule<KjuAlphabet> ExpressionUnaryOperator = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ExpressionUnaryOperator,
            Rhs = Sum(
                KjuAlphabet.ExpressionAccess.ToRegex(),
                Concat(
                    Sum(
                        KjuAlphabet.LogicNot.ToRegex(),
                        KjuAlphabet.Plus.ToRegex(),
                        KjuAlphabet.Minus.ToRegex()),
                    KjuAlphabet.ExpressionUnaryOperator.ToRegex()))
        };

        public static readonly Rule<KjuAlphabet> Literal = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Literal,
            Rhs = Sum(
                KjuAlphabet.DecimalLiteral.ToRegex(),
                KjuAlphabet.BooleanLiteral.ToRegex(),
                KjuAlphabet.NullLiteral.ToRegex(),
                KjuAlphabet.ApplyExpression.ToRegex(),
                KjuAlphabet.UnapplyExpression.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> ApplyExpression = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ApplyExpression,
            Rhs = Concat(
                KjuAlphabet.Apply.ToRegex(),
                KjuAlphabet.LParen.ToRegex(),
                KjuAlphabet.VariableFunctionIdentifier.ToRegex(),
                Concat(
                    KjuAlphabet.Comma.ToRegex(),
                    KjuAlphabet.Expression.ToRegex()).Starred(),
                KjuAlphabet.RParen.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> UnapplyExpression = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.UnapplyExpression,
            Rhs = Concat(
                KjuAlphabet.Unapply.ToRegex(),
                KjuAlphabet.LParen.ToRegex(),
                KjuAlphabet.VariableFunctionIdentifier.ToRegex(),
                KjuAlphabet.RParen.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Alloc = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Alloc,
            Rhs = Concat(
                KjuAlphabet.New.ToRegex(),
                KjuAlphabet.LParen.ToRegex(),
                KjuAlphabet.TypeIdentifier.ToRegex(),
                Concat(
                    KjuAlphabet.Comma.ToRegex(),
                    KjuAlphabet.Expression.ToRegex()).Optional(),
                KjuAlphabet.RParen.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> ParenEnclosedStatement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ParenEnclosedStatement,
            Rhs = Concat(
                KjuAlphabet.LParen.ToRegex(),
                KjuAlphabet.Statement.ToRegex(),
                KjuAlphabet.RParen.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> FieldAccess = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.FieldAccess,
            Rhs = Concat(
                KjuAlphabet.Dot.ToRegex(),
                KjuAlphabet.VariableFunctionIdentifier.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> ArrayAccess = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ArrayAccess,
            Rhs = Concat(
                KjuAlphabet.LBracket.ToRegex(),
                KjuAlphabet.Expression.ToRegex(),
                KjuAlphabet.RBracket.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Access = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Access,
            Rhs = Sum(KjuAlphabet.ArrayAccess.ToRegex(), KjuAlphabet.FieldAccess.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> ExpressionAccess = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ExpressionAccess,
            Rhs = Concat(KjuAlphabet.ExpressionAtom.ToRegex(), KjuAlphabet.Access.ToRegex().Starred())
        };

        public static readonly Rule<KjuAlphabet> ExpressionAtom = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ExpressionAtom,
            Rhs =
                Sum(
                    KjuAlphabet.Alloc.ToRegex(),
                    KjuAlphabet.VariableUse.ToRegex(),
                    KjuAlphabet.ParenEnclosedStatement.ToRegex(),
                    KjuAlphabet.Literal.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> StructField = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.StructField,
            Rhs = Concat(
                KjuAlphabet.VariableFunctionIdentifier.ToRegex(),
                KjuAlphabet.Colon.ToRegex(),
                KjuAlphabet.TypeIdentifier.ToRegex(),
                KjuAlphabet.Semicolon.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> StructFields = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.StructFields,
            Rhs = KjuAlphabet.StructField.ToRegex().Starred()
        };

        public static readonly Rule<KjuAlphabet> StructDefinition = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.StructDefinition,
            Rhs = Concat(
                KjuAlphabet.Struct.ToRegex(),
                KjuAlphabet.TypeIdentifier.ToRegex(),
                KjuAlphabet.LBrace.ToRegex(),
                KjuAlphabet.StructFields.ToRegex(),
                KjuAlphabet.RBrace.ToRegex())
        };

        public static readonly Grammar<KjuAlphabet> Instance = new Grammar<KjuAlphabet>
        {
            StartSymbol = KjuAlphabet.Kju,
            Rules = new ReadOnlyCollection<Rule<KjuAlphabet>>(
                new List<Rule<KjuAlphabet>>
                {
                    Kju,

                    FunctionDefinition,
                    FunctionParameter,
                    FunctionCall,

                    StructDefinition,
                    StructFields,
                    StructField,

                    TypeIdentifier,

                    Alloc,

                    Block,
                    Instruction,
                    NotDelimeteredInstruction,
                    Statement,
                    IfStatement,
                    WhileStatement,
                    ReturnStatement,
                    ApplyExpression,
                    UnapplyExpression,
                    VariableDeclaration,

                    Access,
                    ArrayAccess,
                    FieldAccess,

                    Expression,
                    ExpressionAssignment,
                    ExpressionLogicalOr,
                    ExpressionLogicalAnd,
                    ExpressionEqualsNotEquals,
                    ExpressionLessThanGreaterThan,
                    ExpressionPlusMinus,
                    ExpressionTimesDivideModulo,
                    ExpressionUnaryOperator,
                    ExpressionAccess,
                    ExpressionAtom,

                    ParenEnclosedStatement,

                    VariableUse,
                    Literal
                })
        };

        private static Rule<KjuAlphabet> CreateExpressionRule(
            KjuAlphabet currentRule, KjuAlphabet nextRule, params KjuAlphabet[] operators)
        {
            return new Rule<KjuAlphabet>
            {
                Name = $"ExpressionRule_{currentRule}",
                Lhs = currentRule,
                Rhs = Concat(
                    nextRule.ToRegex(),
                    Concat(
                        Sum(operators.Select(x => x.ToRegex()).ToArray()),
                        currentRule.ToRegex()).Optional())
            };
        }
    }
}