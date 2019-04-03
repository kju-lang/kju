namespace KJU.Core.AST
{
    using Diagnostics;
    using Lexer;
    using Parser;

    public interface IParseTreeToAstConverter<TLabel>
    {
        Node GenerateAst(ParseTree<TLabel> parseTree, IDiagnostics diagnostics);
    }
}