namespace KJU.Core.Compiler
{
    using System;
    using System.Linq;
    using AST;
    using AST.ReturnChecker;
    using AST.TypeChecker;
    using CodeGeneration.AsmHeaderGenerator;
    using CodeGeneration.FunctionToAsmGeneration;
    using Diagnostics;
    using Input;
    using Intermediate.Function;
    using Intermediate.FunctionGeneration.BodyGenerator;
    using Intermediate.FunctionGeneration.FunctionGenerator.Factory;
    using Intermediate.IntermediateRepresentationGenerator;
    using Intermediate.NameMangler;
    using Intermediate.VariableAndFunctionBuilder;
    using KJU.Core.AST.ParseTreeToAstConverter;
    using KJU.Core.CodeGeneration.DataLayout;
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
            new VariableAndFunctionBuilder();

        private readonly IIntermediateRepresentationGenerator intermediateGenerator =
            new IntermediateRepresentationGenerator(new FunctionGeneratorFactory().ConstructGenerator());

        private readonly IFunctionToAsmGenerator
            functionToAsmGenerator = new FunctionToAsmGeneratorFactory().Generate();

        private readonly DataTypeLayoutGenerator dataTypeLayoutGenerator = new DataTypeLayoutGenerator();

        private readonly IAsmHeaderGenerator asmHeaderGenerator = new AsmHeaderGenerator();

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
                var dataSectionHeader = this.asmHeaderGenerator.GenerateDataSectionHeader();
                var dataSection = this.dataTypeLayoutGenerator.GenerateDataLayouts(ast).Prepend(dataSectionHeader);
                var asmHeader = this.asmHeaderGenerator.GenerateHeader();
                var functionsAsm = functionsIR.SelectMany(x => this.functionToAsmGenerator.ToAsm(x.Key, x.Value))
                    .ToList();
                var asm = functionsAsm.Prepend(asmHeader).Concat(dataSection).ToList();
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
