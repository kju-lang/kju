namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;

    public static class KjuLexerFactory
    {
        public static Lexer<KjuAlphabet> CreateLexer()
        {
            var resolver = new ConflictResolver<KjuAlphabet>(KjuAlphabet.None);

            var tokenCategories = new Dictionary<KjuAlphabet, string>
            {
                { KjuAlphabet.Whitespace, "[ \n\r\t\u000b\u000c][ \n\r\t\u000b\u000c]*" },
                { KjuAlphabet.LBrace, "{" },
                { KjuAlphabet.RBrace, "}" },
                { KjuAlphabet.LParen, "\\(" },
                { KjuAlphabet.RParen, "\\)" },
                { KjuAlphabet.LBracket, "\\[" },
                { KjuAlphabet.RBracket, "\\]" },
                { KjuAlphabet.Comma, "," },
                { KjuAlphabet.Colon, ":" },
                { KjuAlphabet.Semicolon, ";" },
                { KjuAlphabet.Dot, "." },
                { KjuAlphabet.If, "if" },
                { KjuAlphabet.Import, "import" },
                { KjuAlphabet.Then, "then" },
                { KjuAlphabet.Else, "else" },
                { KjuAlphabet.While, "while" },
                { KjuAlphabet.Break, "break" },
                { KjuAlphabet.Continue, "continue" },
                { KjuAlphabet.Var, "var" },
                { KjuAlphabet.Fun, "fun" },
                { KjuAlphabet.Struct, "struct" },
                { KjuAlphabet.Return, "return" },
                { KjuAlphabet.DecimalLiteral, "0|[1-9][0-9]*" },
                { KjuAlphabet.BooleanLiteral, "true|false" },
                { KjuAlphabet.TypeIdentifier, "[A-Z][a-zA-Z0-9_]*" },
                { KjuAlphabet.VariableFunctionIdentifier, "[a-z][a-zA-Z0-9_]*" },
                { KjuAlphabet.Equals, "==" },
                { KjuAlphabet.LessOrEqual, "<=" },
                { KjuAlphabet.GreaterOrEqual, ">=" },
                { KjuAlphabet.LessThan, "<" },
                { KjuAlphabet.GreaterThan, ">" },
                { KjuAlphabet.NotEquals, "!=" },
                { KjuAlphabet.LogicNot, "!" },
                { KjuAlphabet.Plus, "\\+" },
                { KjuAlphabet.Minus, "-" },
                { KjuAlphabet.Star, "\\*" },
                { KjuAlphabet.Slash, "/" },
                { KjuAlphabet.Percent, "%" },
                { KjuAlphabet.LogicalAnd, "&&" },
                { KjuAlphabet.LogicalOr, "\\|\\|" },
                { KjuAlphabet.Assign, "=" },
                { KjuAlphabet.PlusAssign, "\\+=" },
                { KjuAlphabet.MinusAssign, "-=" },
                { KjuAlphabet.StarAssign, "\\*=" },
                { KjuAlphabet.SlashAssign, "/=" },
                { KjuAlphabet.PercentAssign, "%=" },
                { KjuAlphabet.New, "new" }
            };

            return new Lexer<KjuAlphabet>(
                tokenCategories,
                KjuAlphabet.Eof,
                KjuAlphabet.None,
                resolver.ResolveWithMinValue);
        }
    }
}