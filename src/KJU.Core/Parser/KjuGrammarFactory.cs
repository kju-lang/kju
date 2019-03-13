namespace KJU.Core.Parser
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Regex;
    using static KjuAlphabet;
    using static Regex.RegexUtils;

    public class KjuGrammarFactory : IGrammarFactory<KjuAlphabet>
    {
        public Grammar<KjuAlphabet> Create()
        {
            var kju = new Rule<KjuAlphabet>
            {
                Lhs = Kju,
                Rhs = new StarRegex<KjuAlphabet>(FunctionDeclaration.ToRegex())
            };

            var function = new Rule<KjuAlphabet>
            {
                Lhs = FunctionDeclaration,
                Rhs = Concat(
                    Fun.ToRegex(),
                    VariableFunctionIdentifier.ToRegex(),
                    LParen.ToRegex(),
                    CreateListRegex(FunctionParameter.ToRegex(), Colon.ToRegex()),
                    RParen.ToRegex(),
                    Colon.ToRegex(),
                    TypeIdentifier.ToRegex(),
                    Block.ToRegex())
            };

            var block = new Rule<KjuAlphabet>
            {
                Lhs = Block,
                Rhs = Concat(LBrace.ToRegex(), Instruction.ToRegex().Starred(), RBrace.ToRegex())
            };

            var instruction = new Rule<KjuAlphabet>
            {
                Lhs = Instruction,
                Rhs = Concat(NotDelimeteredInstruction.ToRegex(), Semicolon.ToRegex())
            };

            var notDelimeteredInstruction = new Rule<KjuAlphabet>
            {
                Lhs = NotDelimeteredInstruction,
                Rhs = Sum(
                    Expression.ToRegex())
            };

            var functionParameter = new Rule<KjuAlphabet>
            {
                Lhs = FunctionParameter,
                Rhs = Concat(VariableFunctionIdentifier.ToRegex(), Colon.ToRegex(), TypeIdentifier.ToRegex())
            };

            var functionCall = new Rule<KjuAlphabet>
            {
                Lhs = FunctionCall,
                Rhs = Concat(
                    VariableFunctionIdentifier.ToRegex(),
                    LParen.ToRegex(),
                    CreateListRegex(Expression.ToRegex(), Comma.ToRegex()),
                    RParen.ToRegex())
            };

            var ifStatement = new Rule<KjuAlphabet>
            {
                Lhs = IfStatement,
                Rhs = Concat(
                    If.ToRegex(),
                    Expression.ToRegex(),
                    Then.ToRegex(),
                    Block.ToRegex(),
                    Else.ToRegex(),
                    Block.ToRegex())
            };

            var whileStatement = new Rule<KjuAlphabet>
            {
                Lhs = WhileStatement,
                Rhs = Concat(
                    While.ToRegex(),
                    Expression.ToRegex(),
                    Block.ToRegex())
            };

            var returnStatement = new Rule<KjuAlphabet>
            {
                Lhs = ReturnStatement,
                Rhs = Concat(
                    Return.ToRegex(),
                    Expression.ToRegex().Optional())
            };

            var variableDeclaration = new Rule<KjuAlphabet>
            {
                Lhs = VariableDeclaration,
                Rhs = Concat(
                    Var.ToRegex(),
                    VariableFunctionIdentifier.ToRegex(),
                    Colon.ToRegex(),
                    TypeIdentifier.ToRegex(),
                    Concat(Assign.ToRegex(), Expression.ToRegex()).Optional())
            };

            var variableAssigment = new Rule<KjuAlphabet>
            {
                Lhs = VariableAssigment,
                Rhs = Concat(
                    VariableFunctionIdentifier.ToRegex(),
                    Sum(
                        Assign.ToRegex(),
                        PlusAssign.ToRegex(),
                        MinusAssign.ToRegex(),
                        StarAssign.ToRegex(),
                        SlashAssign.ToRegex(),
                        PercentAssign.ToRegex()),
                    Expression.ToRegex())
            };

            var expression = new Rule<KjuAlphabet>
            {
                Lhs = Expression,
                Rhs = ExpressionOr.ToRegex()
            };

            var expressionLogicalOr = this.CreateExpressionRule(ExpressionOr, ExpressionAnd, LogicalOr);

            var expressionLogicalAnd = this.CreateExpressionRule(ExpressionAnd, ExpressionEqualsNotEquals, LogicalAnd);

            var expressionEqualsNotEquals =
                this.CreateExpressionRule(
                    ExpressionEqualsNotEquals,
                    ExpressionLessThanGreaterThan,
                    KjuAlphabet.Equals,
                    NotEquals);

            var expressionLessThanGreaterThan = this.CreateExpressionRule(
                ExpressionLessThanGreaterThan,
                ExpressionPlusMinus,
                LessThan,
                LessOrEqual,
                GreaterThan,
                GreaterOrEqual);

            var expressionPlusMinus = this.CreateExpressionRule(
                ExpressionPlusMinus,
                ExpressionTimesDivideModulo,
                Plus,
                Minus);

            var expressionTimesDivideModulo = this.CreateExpressionRule(
                ExpressionTimesDivideModulo,
                ExpressionLogicalNot,
                Star,
                Slash,
                Percent);

            var expressionLogicalNot = new Rule<KjuAlphabet>
            {
                Lhs = ExpressionLogicalNot,
                Rhs = Sum(
                    ExpressionAtom.ToRegex(),
                    Concat(
                        LogicNot.ToRegex(),
                        ExpressionLogicalNot.ToRegex()))
            };

            var literal = new Rule<KjuAlphabet>
            {
                Lhs = Literal,
                Rhs = Sum(DecimalLiteral.ToRegex(), BooleanLiteral.ToRegex())
            };

            var expressionAtom = new Rule<KjuAlphabet>
            {
                Lhs = ExpressionAtom,
                Rhs = Sum(
                    VariableDeclaration.ToRegex(),
                    VariableAssigment.ToRegex(),
                    FunctionCall.ToRegex(),
                    ReturnStatement.ToRegex(),
                    IfStatement.ToRegex(),
                    WhileStatement.ToRegex(),
                    Block.ToRegex(),
                    Break.ToRegex(),
                    Continue.ToRegex(),
                    Literal.ToRegex(),
                    FunctionDeclaration.ToRegex(),
                    Concat(
                        LParen.ToRegex(),
                        Expression.ToRegex(),
                        RParen.ToRegex()))
            };

            var rules = new List<Rule<KjuAlphabet>>
            {
                kju, function, block, instruction, notDelimeteredInstruction, functionParameter, functionCall,
                ifStatement, whileStatement, returnStatement, variableDeclaration, variableAssigment, expression,
                expressionLogicalOr, expressionLogicalAnd, expressionEqualsNotEquals, expressionLessThanGreaterThan,
                expressionPlusMinus, expressionTimesDivideModulo, expressionLogicalNot, literal, expressionAtom
            };

            return new Grammar<KjuAlphabet>
            {
                StartSymbol = Kju,
                Rules = new ReadOnlyCollection<Rule<KjuAlphabet>>(rules)
            };
        }

        private Rule<KjuAlphabet> CreateExpressionRule(
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