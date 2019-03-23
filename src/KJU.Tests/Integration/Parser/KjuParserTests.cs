namespace KJU.Tests.Integration.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class KjuParserTests
    {
        [TestMethod]
        public void EmptyProgramTest()
        {
            var tree = KjuParserFactory.Instance.Parse(
                new List<Token<KjuAlphabet>> { new Token<KjuAlphabet> { Category = KjuAlphabet.Eof } },
                null);
        }

        [TestMethod]
        public void SimpleProgramTest()
        {
            var tree = KjuParserFactory.Instance.Parse(
                new List<Token<KjuAlphabet>> {
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
            var tree = KjuParserFactory.Instance.Parse("fun kju():Unit{var x:Int=2+2;}", null);
            Assert.AreEqual(
                actual: tree.ToString(),
                expected: "Kju [FunctionDeclaration [Fun'fun', VariableFunctionIdentifier'kju', LParen'(', RParen')', Colon':', TypeIdentifier'Unit', Block [LBrace'{', Instruction [NotDelimeteredInstruction [Statement [VariableDeclaration [Var'var', VariableFunctionIdentifier'x', Colon':', TypeIdentifier'Int', Assign'=', Expression [ExpressionOr [ExpressionAnd [ExpressionEqualsNotEquals [ExpressionLessThanGreaterThan [ExpressionPlusMinus [ExpressionTimesDivideModulo [ExpressionLogicalNot [ExpressionAtom [Literal [DecimalLiteral'2']]]], Plus'+', ExpressionPlusMinus [ExpressionTimesDivideModulo [ExpressionLogicalNot [ExpressionAtom [Literal [DecimalLiteral'2']]]]]]]]]]]]]], Semicolon';'], RBrace'}']]]");
        }
    }
}
