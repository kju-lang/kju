namespace KJU.Core.AST
{
    using System;
    using Diagnostics;
    using Lexer;
    using Parser;

    public class KjuParseTreeToAstConverter : IParseTreeToAstConverter<KjuAlphabet>
    {
        public Node GenerateAst(ParseTree<KjuAlphabet> parseTree, IDiagnostics diagnostics)
        {
            throw new NotImplementedException();
        }
    }
}