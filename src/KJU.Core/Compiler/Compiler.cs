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
        private INameResolver nameResolver;
        private ITypeChecker typeChecker;
        private IParseTreeToAstConverter<KjuAlphabet> parseTreeToAstConverter;
        private Parser<KjuAlphabet> parser;

        public Compiler(Parser<KjuAlphabet> parser, INameResolver nameResolver, ITypeChecker typeChecker, IParseTreeToAstConverter<KjuAlphabet> parseTreeToAstConverter)
        {
            this.parser = parser;
            this.nameResolver = nameResolver;
            this.typeChecker = typeChecker;
            this.parseTreeToAstConverter = parseTreeToAstConverter;
        }

        public Compiler()
            : this(KjuParserFactory.Instance, new NameResolver(), new TypeChecker(), new KjuParseTreeToAstConverter())
        {
        }

        public void Run(string path, IDiagnostics diag)
        {
            string data = File.ReadAllText(path);
            Console.WriteLine($"compiling {data}...");
            var tree = this.parser.Parse(data, diag);
            Console.WriteLine($"tree: {tree}");
            var ast = this.parseTreeToAstConverter.GenerateAst(tree, diag);
            Console.WriteLine($"ast: {ast}");
            this.nameResolver.LinkNames(ast, diag);
            this.typeChecker.LinkTypes(ast, diag);
        }
    }
}