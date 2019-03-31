namespace KJU.Core.Compiler
{
    using System;
    using System.IO;
    using KJU.Core;
    using KJU.Core.AST;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;

    public class Compiler : ICompiler
    {
        private readonly Parser<KjuAlphabet> parser;
        private readonly IParseTreeToAstConverter<KjuAlphabet> parseTreeToAstConverter;
        private readonly INameResolver nameResolver;
        private readonly ITypeChecker typeChecker;

        public Compiler(
            Parser<KjuAlphabet> parser,
            IParseTreeToAstConverter<KjuAlphabet> parseTreeToAstConverter,
            INameResolver nameResolver,
            ITypeChecker typeChecker)
        {
            this.parser = parser;
            this.nameResolver = nameResolver;
            this.typeChecker = typeChecker;
            this.parseTreeToAstConverter = parseTreeToAstConverter;
        }

        public Compiler()
            : this(KjuParserFactory.Instance, new KjuParseTreeToAstConverter(), new NameResolver(), new TypeChecker())
        {
        }

        public void Run(string path, IDiagnostics diag)
        {
            try
            {
                var data = new FileInputReader(path).Read();
                var tree = this.parser.Parse(data, diag);
                var ast = this.parseTreeToAstConverter.GenerateAst(tree, diag);
                this.nameResolver.LinkNames(ast, diag);
                this.typeChecker.LinkTypes(ast, diag);
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
