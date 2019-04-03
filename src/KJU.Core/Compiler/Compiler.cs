namespace KJU.Core.Compiler
{
    using System;
    using System.IO;
    using AST;
    using AST.ReturnChecker;
    using Diagnostics;
    using Input;
    using Lexer;
    using Parser;

    public class Compiler : ICompiler
    {
        private readonly Parser<KjuAlphabet> parser = KjuParserFactory.Instance;

        private readonly IParseTreeToAstConverter<KjuAlphabet> parseTreeToAstConverter =
            new KjuParseTreeToAstConverter();

        private readonly IPhase nameResolver = new NameResolver();
        private readonly IPhase typeChecker = new TypeChecker();
        private readonly IPhase returnChecker = new ReturnChecker();

        public void Run(string path, IDiagnostics diag)
        {
            var data = File.ReadAllText(path);
            this.RunOnText(data, diag);
        }

        public void RunOnText(string data, IDiagnostics diag)
        {
            try
            {
                var tree = this.parser.Parse(data, diag);
                var ast = this.parseTreeToAstConverter.GenerateAst(tree, diag);
                this.nameResolver.Run(ast, diag);
                this.typeChecker.Run(ast, diag);
                this.returnChecker.Run(ast, diag);
            }
            catch (Exception ex) when (
                ex is ParseException
                || ex is FormatException
                || ex is PreprocessorException
                || ex is ParseTreeToAstConverterException
                || ex is NameResolverException
                || ex is TypeCheckerException)
            {
                throw new CompilerException("Compilation failed.", ex);
            }
        }
    }
}