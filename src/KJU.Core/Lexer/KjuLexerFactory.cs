namespace KJU.Core.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Input;

    public class KjuLexerFactory
    {
        public static readonly Lexer<KjuAlphabet> Instance = CreateLexer();

        private static Lexer<KjuAlphabet> CreateLexer()
        {
            var resolver = new ConflictResolver<KjuAlphabet>(KjuAlphabet.None);

            var tokenCategories = new Dictionary<KjuAlphabet, string>
            {
                { KjuAlphabet.Whitespace, "[ \n\r\t][ \n\r\t]*" },
                { KjuAlphabet.LBrace, "{" },
                { KjuAlphabet.RBrace, "}" },
                { KjuAlphabet.LParen, "\\(" },
                { KjuAlphabet.RParen, "\\)" },
                { KjuAlphabet.Comma, "," },
                { KjuAlphabet.Colon, ":" },
                { KjuAlphabet.Semicolon, ";" },
                { KjuAlphabet.If, "if" },
                { KjuAlphabet.Then, "then" },
                { KjuAlphabet.Else, "else" },
                { KjuAlphabet.While, "while" },
                { KjuAlphabet.Break, "break" },
                { KjuAlphabet.Continue, "continue" },
                { KjuAlphabet.Var, "var" },
                { KjuAlphabet.Fun, "fun" },
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
                { KjuAlphabet.PercentAssign, "%=" }
            };

            var lexer = new Lexer<KjuAlphabet>(tokenCategories, KjuAlphabet.Eof, KjuAlphabet.None, resolver.ResolveWithMinValue);
            return lexer;
        }
    }
}