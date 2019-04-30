namespace KJU.Core.Compiler
{
    using System;
    using System.Linq;
    using AST;
    using AST.ReturnChecker;
    using AST.TypeChecker;
    using CodeGeneration.FunctionToAsmGeneration;
    using Diagnostics;
    using Input;
    using Intermediate.FunctionBodyGenerator;
    using Intermediate.IntermediateRepresentationGenerator;
    using Intermediate.NameMangler;
    using Intermediate.VariableAndFunctionBuilder;
    using Lexer;
    using Parser;

    public class Compiler : ICompiler
    {
        private readonly Preprocessor preprocessor = new Preprocessor();
        private readonly Lexer<KjuAlphabet> lexer = KjuLexerFactory.CreateLexer();

        private readonly Parser<KjuAlphabet> parser =
            ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);

        private readonly IParseTreeToAstConverter<KjuAlphabet> parseTreeToAstConverter =
            new KjuParseTreeToAstConverter();

        private readonly IPhase nameResolver = new NameResolver();
        private readonly IPhase typeChecker = new TypeChecker();
        private readonly IPhase returnChecker = new ReturnChecker();

        private readonly IVariableAndFunctionBuilder variableAndFunctionBuilder =
            new VariableAndFunctionBuilder(new NameMangler());

        private readonly IIntermediateRepresentationGenerator intermediateGenerator =
            new IntermediateRepresentationGenerator();

        private readonly IFunctionToAsmGenerator functionToAsmGenerator = new FunctionToAsmGeneratorFactory().Generate();

        public Artifacts RunOnInputReader(IInputReader inputReader, IDiagnostics diagnostics)
        {
            try
            {
                var input = inputReader.Read();
                var preprocessed = this.preprocessor.PreprocessInput(input, diagnostics);
                var tokens = this.lexer.Scan(preprocessed, diagnostics);
                var tokensFiltered = tokens.Where(x => !KjuAlphabet.Whitespace.Equals(x.Category));
                var tree = this.parser.Parse(tokensFiltered, diagnostics);
                var ast = this.parseTreeToAstConverter.GenerateAst(tree, diagnostics);
                this.nameResolver.Run(ast, diagnostics);
                this.typeChecker.Run(ast, diagnostics);
                this.returnChecker.Run(ast, diagnostics);
                this.variableAndFunctionBuilder.BuildFunctionsAndVariables(ast);
                var functionsIR = this.intermediateGenerator.CreateIR(ast);
                var asm = functionsIR.Keys.SelectMany(this.functionToAsmGenerator.ToAsm);
                return new Artifacts(ast, asm);
            }
            catch (Exception ex) when (
                ex is PreprocessorException
                || ex is LexerException
                || ex is ParseException
                || ex is ParseTreeToAstConverterException
                || ex is NameResolverException
                || ex is TypeCheckerException
                || ex is ReturnCheckerException
                || ex is FunctionBodyGeneratorException)
            {
                throw new CompilerException("Compilation failed.", ex);
            }
        }
    }
}