namespace KJU.Core.Parser
{
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
                Rhs = new StarRegex<KjuAlphabet>(Function.ToRegex())
            };

            var function = new Rule<KjuAlphabet>
            {
                Lhs = Function,
                Rhs = Concat(
                    Fun.ToRegex(),
                    VariableFunctionIdentifier.ToRegex(),
                    LParen.ToRegex(),
                    Concat(
                        FunctionParameter.ToRegex(),
                        Concat(Comma.ToRegex(), FunctionParameter.ToRegex()).Starred()).Optional(),
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
                    VariableDeclaration.ToRegex(),
                    VariableDeclarationAndAssigment.ToRegex(),
                    VariableAssigment.ToRegex(),
                    FunctionCall.ToRegex(),
                    Expression.ToRegex(),
                    ReturnStatement.ToRegex(),
                    IfStatement.ToRegex(),
                    WhileStatement.ToRegex())
            };

            var functionParameter = new Rule<KjuAlphabet>
            {
                Lhs = FunctionParameter,
                Rhs = Concat(VariableFunctionIdentifier.ToRegex(), Colon.ToRegex(), TypeIdentifier.ToRegex())
            };

            return new Grammar<KjuAlphabet> { StartSymbol = Kju };
        }
    }
}