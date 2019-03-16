namespace KJU.Core.Parser
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Regex;
    using static KjuAlphabet;
    using static Regex.RegexUtils;

    public static class KjuGrammar
    {
        public static readonly Rule<KjuAlphabet> Kju = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Kju,
            Rhs = new StarRegex<KjuAlphabet>(FunctionDeclaration.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Function = new Rule<KjuAlphabet>
        {
            Lhs = FunctionDeclaration,
            Rhs = Concat(
                Fun.ToRegex(),
                VariableFunctionIdentifier.ToRegex(),
                LParen.ToRegex(),
                CreateListRegex(KjuAlphabet.FunctionParameter.ToRegex(), Colon.ToRegex()),
                RParen.ToRegex(),
                Colon.ToRegex(),
                TypeIdentifier.ToRegex(),
                KjuAlphabet.Block.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Block = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Block,
            Rhs = Concat(LBrace.ToRegex(), KjuAlphabet.Instruction.ToRegex().Starred(), RBrace.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Instruction = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Instruction,
            Rhs = Concat(KjuAlphabet.NotDelimeteredInstruction.ToRegex(), Semicolon.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> NotDelimeteredInstruction = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.NotDelimeteredInstruction,
            Rhs = Sum(
                KjuAlphabet.Expression.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> FunctionParameter = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.FunctionParameter,
            Rhs = Concat(VariableFunctionIdentifier.ToRegex(), Colon.ToRegex(), TypeIdentifier.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> FunctionCall = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.FunctionCall,
            Rhs = Concat(
                VariableFunctionIdentifier.ToRegex(),
                LParen.ToRegex(),
                CreateListRegex(KjuAlphabet.Expression.ToRegex(), Comma.ToRegex()),
                RParen.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> IfStatement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.IfStatement,
            Rhs = Concat(
                If.ToRegex(),
                KjuAlphabet.Expression.ToRegex(),
                Then.ToRegex(),
                KjuAlphabet.Block.ToRegex(),
                Else.ToRegex(),
                KjuAlphabet.Block.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> WhileStatement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.WhileStatement,
            Rhs = Concat(
                While.ToRegex(),
                KjuAlphabet.Expression.ToRegex(),
                KjuAlphabet.Block.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> ReturnStatement = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ReturnStatement,
            Rhs = Concat(
                Return.ToRegex(),
                KjuAlphabet.Expression.ToRegex()) // .Optional()
        };

        public static readonly Rule<KjuAlphabet> VariableDeclaration = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.VariableDeclaration,
            Rhs = Concat(
                Var.ToRegex(),
                VariableFunctionIdentifier.ToRegex(),
                Colon.ToRegex(),
                TypeIdentifier.ToRegex(),
                Concat(Assign.ToRegex(), KjuAlphabet.Expression.ToRegex()).Optional())
        };

        public static readonly Rule<KjuAlphabet> VariableAssigment = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.VariableAssigment,
            Rhs = Concat(
                VariableFunctionIdentifier.ToRegex(),
                Sum(
                    Assign.ToRegex(),
                    PlusAssign.ToRegex(),
                    MinusAssign.ToRegex(),
                    StarAssign.ToRegex(),
                    SlashAssign.ToRegex(),
                    PercentAssign.ToRegex()),
                KjuAlphabet.Expression.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> Expression = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Expression,
            Rhs = ExpressionOr.ToRegex()
        };

        public static readonly Rule<KjuAlphabet> ExpressionLogicalOr =
            CreateExpressionRule(ExpressionOr, ExpressionAnd, LogicalOr);

        public static readonly Rule<KjuAlphabet> ExpressionLogicalAnd =
            CreateExpressionRule(ExpressionAnd, KjuAlphabet.ExpressionEqualsNotEquals, LogicalAnd);

        public static readonly Rule<KjuAlphabet> ExpressionEqualsNotEquals =
            CreateExpressionRule(
                KjuAlphabet.ExpressionEqualsNotEquals,
                KjuAlphabet.ExpressionLessThanGreaterThan,
                KjuAlphabet.Equals,
                NotEquals);

        public static readonly Rule<KjuAlphabet> ExpressionLessThanGreaterThan = CreateExpressionRule(
            KjuAlphabet.ExpressionLessThanGreaterThan,
            KjuAlphabet.ExpressionPlusMinus,
            LessThan,
            LessOrEqual,
            GreaterThan,
            GreaterOrEqual);

        public static readonly Rule<KjuAlphabet> ExpressionPlusMinus = CreateExpressionRule(
            KjuAlphabet.ExpressionPlusMinus,
            KjuAlphabet.ExpressionTimesDivideModulo,
            Plus,
            Minus);

        public static readonly Rule<KjuAlphabet> ExpressionTimesDivideModulo = CreateExpressionRule(
            KjuAlphabet.ExpressionTimesDivideModulo,
            KjuAlphabet.ExpressionLogicalNot,
            Star,
            Slash,
            Percent);

        public static readonly Rule<KjuAlphabet> ExpressionLogicalNot = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ExpressionLogicalNot,
            Rhs = Sum(
                KjuAlphabet.ExpressionAtom.ToRegex(),
                Concat(
                    LogicNot.ToRegex(),
                    KjuAlphabet.ExpressionLogicalNot.ToRegex()))
        };

        public static readonly Rule<KjuAlphabet> Literal = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.Literal,
            Rhs = Sum(DecimalLiteral.ToRegex(), BooleanLiteral.ToRegex())
        };

        public static readonly Rule<KjuAlphabet> ExpressionAtom = new Rule<KjuAlphabet>
        {
            Lhs = KjuAlphabet.ExpressionAtom,
            Rhs = Sum(
                VariableFunctionIdentifier.ToRegex(),
                KjuAlphabet.VariableDeclaration.ToRegex(),
                KjuAlphabet.VariableAssigment.ToRegex(),
                KjuAlphabet.FunctionCall.ToRegex(),
                KjuAlphabet.ReturnStatement.ToRegex(),
                KjuAlphabet.IfStatement.ToRegex(),
                KjuAlphabet.WhileStatement.ToRegex(),
                KjuAlphabet.Block.ToRegex(),
                Break.ToRegex(),
                Continue.ToRegex(),
                KjuAlphabet.Literal.ToRegex(),
                FunctionDeclaration.ToRegex(),
                Concat(
                    LParen.ToRegex(),
                    KjuAlphabet.Expression.ToRegex(),
                    RParen.ToRegex()))
        };

        public static readonly Grammar<KjuAlphabet> Instance = new Grammar<KjuAlphabet>
        {
            StartSymbol = KjuAlphabet.Kju,
            Rules = new ReadOnlyCollection<Rule<KjuAlphabet>>(new List<Rule<KjuAlphabet>>
            {
                Kju, Function, Block, Instruction, NotDelimeteredInstruction, FunctionParameter, FunctionCall,
                IfStatement, WhileStatement, ReturnStatement, VariableDeclaration, VariableAssigment, Expression,
                ExpressionLogicalOr, ExpressionLogicalAnd, ExpressionEqualsNotEquals, ExpressionLessThanGreaterThan,
                ExpressionPlusMinus, ExpressionTimesDivideModulo, ExpressionLogicalNot, Literal, ExpressionAtom
            })
        };

        private static Rule<KjuAlphabet> CreateExpressionRule(
            KjuAlphabet currentRule, KjuAlphabet nextRule, params KjuAlphabet[] operators)
        {
            return new Rule<KjuAlphabet>
            {
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