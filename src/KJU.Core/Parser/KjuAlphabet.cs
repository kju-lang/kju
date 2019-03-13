namespace KJU.Core.Parser
{
    public enum KjuAlphabet
    {
        // Terminals:

        /// <summary>
        /// {
        /// </summary>
        LBrace,

        /// <summary>
        /// }
        /// </summary>
        RBrace,

        /// <summary>
        /// (
        /// </summary>
        LParen,

        /// <summary>
        /// )
        /// </summary>
        RParen,

        /// <summary>
        /// \/\*
        /// </summary>
        LComment,

        /// <summary>
        /// \*\/
        /// </summary>
        RComment,

        /// <summary>
        /// ,
        /// </summary>
        Comma,

        /// <summary>
        /// :
        /// </summary>
        Colon,

        /// <summary>
        /// ;
        /// </summary>
        Semicolon,

        /// <summary>
        /// if
        /// </summary>
        If,

        /// <summary>
        /// then
        /// </summary>
        Then,

        /// <summary>
        /// else
        /// </summary>
        Else,

        /// <summary>
        /// while
        /// </summary>
        While,

        /// <summary>
        /// break
        /// </summary>
        Break,

        /// <summary>
        /// continue
        /// </summary>
        Continue,

        /// <summary>
        /// var
        /// </summary>
        Var,

        /// <summary>
        /// fun
        /// </summary>
        Fun,

        /// <summary>
        /// return
        /// </summary>
        Return,

        /// <summary>
        /// 0|[1-9][0-9]*
        /// </summary>
        DecimalValue,

        /// <summary>
        /// true|false
        /// </summary>
        BooleanValue,

        /// <summary>
        /// [A-Z][a-zA-Z0-9_]*
        /// </summary>
        TypeIdentifier,

        /// <summary>
        /// [a-z][a-zA-Z0-9_]*
        /// </summary>
        VariableFunctionIdentifier,

        /// <summary>
        /// =
        /// </summary>
        Assign,

        /// <summary>
        /// ==
        /// </summary>
        Equals,

        /// <summary>
        /// &lt;
        /// </summary>
        LessThan,

        /// <summary>
        /// &gt;
        /// </summary>
        GreaterThan,

        /// <summary>
        /// &lt;=
        /// </summary>
        LessEqual,

        /// <summary>
        /// &gt;=
        /// </summary>
        GreaterEqual,

        /// <summary>
        /// !
        /// </summary>
        LogicNot,

        /// <summary>
        /// !=
        /// </summary>
        NotEquals,

        /// <summary>
        /// +
        /// </summary>
        Plus,

        /// <summary>
        /// -
        /// </summary>
        Minus,

        /// <summary>
        /// *
        /// </summary>
        Star,

        /// <summary>
        /// /
        /// </summary>
        Slash,

        /// <summary>
        /// %
        /// </summary>
        Percent,

        /// <summary>
        /// &
        /// </summary>
        And,

        /// <summary>
        /// |
        /// </summary>
        Or,

        /// <summary>
        /// +=
        /// </summary>
        PlusAssign,

        /// <summary>
        /// -=
        /// </summary>
        MinusAssign,

        /// <summary>
        /// *=
        /// </summary>
        StarAssign,

        /// <summary>
        /// /=
        /// </summary>
        SlashAssign,

        /// <summary>
        /// %=
        /// </summary>
        PercentAssign,

        // Nonterminals:

        /// <summary>
        /// Starting symbol
        /// </summary>
        Kju,

        /// <summary>
        /// Whole function
        /// </summary>
        Function,

        /// <summary>
        /// Block of code delimetered by {}
        /// </summary>
        Block,

        /// <summary>
        /// Instruction ending by ;
        /// </summary>
        Instruction,

        /// <summary>
        /// Instruction not ending by ;
        /// </summary>
        NotDelimeteredInstruction,

        VariableDeclaration,

        VariableDeclarationAndAssigment,

        VariableAssigment,

        FunctionCall,

        FunctionParameter,

        Expression,

        ReturnStatement,

        IfStatement,

        WhileStatement,
    }
}