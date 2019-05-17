namespace KJU.Tests.Integration.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class KjuParserTests
    {
        private readonly Parser<KjuAlphabet> parser = ParserFactory<KjuAlphabet>.MakeParser(KjuGrammar.Instance, KjuAlphabet.Eof);

        [TestMethod]
        public void EmptyProgramTest()
        {
            this.parser.Parse(
                new List<Token<KjuAlphabet>> { new Token<KjuAlphabet> { Category = KjuAlphabet.Eof } },
                null);
        }

        [TestMethod]
        public void SimpleProgramTest()
        {
            this.parser.Parse(
                new List<Token<KjuAlphabet>>
                {
                    new Token<KjuAlphabet> { Category = KjuAlphabet.Fun },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.VariableFunctionIdentifier, Text = "foo" },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.LParen },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.RParen },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.Colon },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.TypeIdentifier },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.LBrace },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.RBrace },
                    new Token<KjuAlphabet> { Category = KjuAlphabet.Eof },
                }, null);
        }

        [TestMethod]
        public void EndToEndTest()
        {
            var tree = KjuCompilerUtils.Parse("fun kju():Unit{var x:Int=2+2;}", null);
            Assert.AreEqual(
                actual: tree.ToString(),
                expected: "Kju [FunctionDefinition [Fun'fun', VariableFunctionIdentifier'kju', LParen'(', RParen')', Colon':', TypeIdentifier'Unit', Block [LBrace'{', Instruction [NotDelimeteredInstruction [Statement [VariableDeclaration [Var'var', VariableFunctionIdentifier'x', Colon':', TypeIdentifier'Int', Assign'=', Expression [ExpressionAssignment [ExpressionOr [ExpressionAnd [ExpressionEqualsNotEquals [ExpressionLessThanGreaterThan [ExpressionPlusMinus [ExpressionTimesDivideModulo [ExpressionUnaryOperator [ExpressionAccess [ExpressionAtom [Literal [DecimalLiteral'2']]]]], Plus'+', ExpressionPlusMinus [ExpressionTimesDivideModulo [ExpressionUnaryOperator [ExpressionAccess [ExpressionAtom [Literal [DecimalLiteral'2']]]]]]]]]]]]]]]], Semicolon';'], RBrace'}']]]");
        }
    }
}