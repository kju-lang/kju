namespace KJU.Core.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AST;
    using AST.ReturnChecker;
    using AST.TypeChecker;
    using CodeGeneration.FunctionToAsmGeneration;
    using Diagnostics;
    using Input;
    using Intermediate;
    using KJU.Core.Intermediate.FunctionBodyGenerator;
    using KJU.Core.Intermediate.IntermediateRepresentationGenerator;
    using KJU.Core.Intermediate.NameMangler;
    using KJU.Core.Intermediate.VariableAndFunctionBuilder;
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
        private readonly IFunctionToAsmGenerator functionToAsmGenerator = new FunctionToAsmGenerator();

        private readonly IntermediateRepresentationGenerator intermediateGenerator =
            new IntermediateRepresentationGenerator();

        public Artifacts RunOnInputReader(IInputReader inputReader, IDiagnostics diagnostics)
        {
            var artifacts = new Artifacts();

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
                var nameMangler = new NameMangler();
                new VariableAndFunctionBuilder(nameMangler).BuildFunctionsAndVariables(ast);
                var funcionsIR = this.intermediateGenerator.CreateIR(ast);

               /****************
                *  Dirty code  *
                ****************/

                var asm = new List<string>();

                foreach (var entry in funcionsIR)
                {
                    var functionDeclaration = entry.Key;

                    asm.AddRange(this.functionToAsmGenerator.ToAsm(functionDeclaration));
                }

                artifacts.Asm = asm;

               /***********************
                *  End of dirty code  *
                ***********************/

                artifacts.Ast = ast;
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

            return artifacts;
        }
    }
}
