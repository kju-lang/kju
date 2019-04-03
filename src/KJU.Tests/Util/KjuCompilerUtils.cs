namespace KJU.Tests.Util
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Diagnostics;
    using KJU.Core.AST;
    using KJU.Core.AST.ReturnChecker;
    using KJU.Core.AST.TypeChecker;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;

    public static class KjuCompilerUtils
    {
        private static readonly Preprocessor Preprocessor = new Preprocessor();
        private static readonly Lexer<KjuAlphabet> KjuLexer = KjuLexerFactory.CreateLexer();
        private static readonly Parser<KjuAlphabet> KjuParser = KjuParserFactory.CreateParser();

        private static readonly KjuParseTreeToAstConverter
            KjuParseTreeToAstConverter = new KjuParseTreeToAstConverter();

        private static readonly NameResolver NameResolver = new NameResolver();
        private static readonly TypeChecker TypeChecker = new TypeChecker();
        private static readonly ReturnChecker ReturnChecker = new ReturnChecker();

        public static List<KeyValuePair<ILocation, char>> Read(string programText)
        {
            return new StringInputReader(programText).Read();
        }

        public static IEnumerable<KeyValuePair<ILocation, char>> Preprocess(
            string programText,
            IDiagnostics diag)
        {
            return Preprocessor.PreprocessInput(Read(programText), diag);
        }

        public static IEnumerable<Token<KjuAlphabet>> Tokenize(
            string programText,
            IDiagnostics diag)
        {
            return KjuLexer.Scan(Read(programText), diag);
        }

        public static IEnumerable<Token<KjuAlphabet>> TokenizeAndFilter(
            string programText,
            IDiagnostics diag)
        {
            return Tokenize(programText, diag).Where(x => !KjuAlphabet.Whitespace.Equals(x.Category));
        }

        public static ParseTree<KjuAlphabet> Parse(
            string programText,
            IDiagnostics diagnostics)
        {
            return KjuParser.Parse(TokenizeAndFilter(programText, diagnostics), diagnostics);
        }

        public static Node GenerateAst(
            string programText,
            IDiagnostics diagnostics)
        {
            return KjuParseTreeToAstConverter.GenerateAst(Parse(programText, diagnostics), diagnostics);
        }

        public static Node MakeAstWithLinkedNames(
            string programText,
            IDiagnostics diagnostics)
        {
            var ast = GenerateAst(programText, diagnostics);
            NameResolver.Run(ast, diagnostics);
            return ast;
        }

        public static Node MakeAstWithTypesResolved(
            string programText,
            IDiagnostics diagnostics)
        {
            var ast = MakeAstWithLinkedNames(programText, diagnostics);
            TypeChecker.Run(ast, diagnostics);
            return ast;
        }

        public static Node MakeAstWithReturnsChecked(
            string programText,
            IDiagnostics diagnostics)
        {
            var ast = MakeAstWithTypesResolved(programText, diagnostics);
            ReturnChecker.Run(ast, diagnostics);
            return ast;
        }
    }
}