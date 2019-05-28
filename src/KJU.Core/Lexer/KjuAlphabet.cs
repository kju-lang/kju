namespace KJU.Core.Lexer
{
    public enum KjuAlphabet
    {
        /// <summary>
        /// Special symbol for lexer
        /// </summary>
        None = 0,

        /// <summary>
        /// Whitespace (only for lexer)
        /// </summary>
        Whitespace,

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
        /// [
        /// </summary>
        LBracket,

        /// <summary>
        /// ]
        /// </summary>
        RBracket,

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
        /// .
        /// </summary>
        Dot,

        /// <summary>
        /// if
        /// </summary>
        If,

        /// <summary>
        /// import
        /// </summary>
        Import,

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
        /// new
        /// </summary>
        New,

        /// <summary>
        /// var
        /// </summary>
        Var,

        /// <summary>
        /// fun
        /// </summary>
        Fun,

        /// <summary>
        /// apply
        /// </summary>
        Apply,

        /// <summary>
        /// unapply
        /// </summary>
        Unapply,

        /// <summary>
        /// struct
        /// </summary>
        Struct,

        /// <summary>
        /// return
        /// </summary>
        Return,

        /// <summary>
        /// 0|[1-9][0-9]*
        /// </summary>
        DecimalLiteral,

        /// <summary>
        /// true|false
        /// </summary>
        BooleanLiteral,

        /// <summary>
        /// null
        /// </summary>
        NullLiteral,

        /// <summary>
        /// [A-Z][a-zA-Z0-9_]*
        /// </summary>
        TypeIdentifier,

        /// <summary>
        /// [a-z][a-zA-Z0-9_]*
        /// </summary>
        VariableFunctionIdentifier,

        /// <summary>
        /// ->
        /// </summary>
        Arrow,

        /// <summary>
        /// ==
        /// </summary>
        Equals,

        /// <summary>
        /// &lt;=
        /// </summary>
        LessOrEqual,

        /// <summary>
        /// &gt;=
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// &lt;
        /// </summary>
        LessThan,

        /// <summary>
        /// &gt;
        /// </summary>
        GreaterThan,

        /// <summary>
        /// !=
        /// </summary>
        NotEquals,

        /// <summary>
        /// !
        /// </summary>
        LogicNot,

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
        /// &&
        /// </summary>
        LogicalAnd,

        /// <summary>
        /// ||
        /// </summary>
        LogicalOr,

        /// <summary>
        /// =
        /// </summary>
        Assign,

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
        FunctionDefinition,

        /// <summary>
        /// apply(f,x,y,z)
        /// </summary>
        ApplyExpression,

        /// <summary>
        /// unapply(f)
        /// </summary>
        UnapplyExpression,

        /// <summary>
        /// Whole struct
        /// </summary>
        StructDefinition,

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

        /// <summary>
        /// var a:Int = 5 ('= 5' is optional)
        /// </summary>
        VariableDeclaration,

        /// <summary>
        /// One of three:
        /// Assigment: 'a = 5' or 'a+=5' or variation,
        /// function call: 'a(1,2,3);',
        /// or value read: 'a;'
        /// </summary>
        VariableUse,

        /// <summary>
        /// f(5,7,13)
        /// </summary>
        FunctionCall,

        /// <summary>
        /// x:Int
        /// </summary>
        FunctionParameter,

        /// <summary>
        /// return 5 ('5' is optional)
        /// </summary>
        ReturnStatement,

        /// <summary>
        /// if a == b then
        /// {
        ///     3;
        /// }
        /// else
        /// {
        ///     5;
        /// }
        /// </summary>
        IfStatement,

        /// <summary>
        /// while a == b
        /// {
        ///     5;
        /// }
        /// </summary>
        WhileStatement,

        /// <summary>
        /// For array allocation
        /// new(Int, 5)
        /// For struct allocation
        /// new(A)
        /// </summary>
        Alloc,

        /// <summary>
        /// [5]
        /// or
        /// .abc
        /// </summary>
        Access,

        /// <summary>
        /// [5]
        /// </summary>
        ArrayAccess,

        /// <summary>
        /// .abc
        /// </summary>
        FieldAccess,

        /// <summary>
        /// abc : Int;
        /// def : Unit;
        /// ghj : Bool;
        /// </summary>
        StructFields,

        /// <summary>
        /// abc : Int;
        /// </summary>
        StructField,

        Expression,
        ExpressionAssignment,
        ExpressionOr,
        ExpressionAnd,
        ExpressionEqualsNotEquals,
        ExpressionLessThanGreaterThan,
        ExpressionPlusMinus,
        ExpressionTimesDivideModulo,
        ExpressionUnaryOperator,
        ExpressionAccess,
        ExpressionAtom,

        /// <summary>
        /// Decimal or Boolean value
        /// </summary>
        Literal,
        Statement,
        ParenEnclosedStatement,

        Eof,
    }
}