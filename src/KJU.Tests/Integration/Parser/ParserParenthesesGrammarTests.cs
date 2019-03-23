namespace KJU.Tests.Integration.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using KJU.Core.Diagnostics;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Core.Util;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class ParserParenthesesGrammarTests
    {
        private enum ParenAlphabet
        {
            L,
            R,
            S,
            EOF
        }

        [TestMethod]
        public void TestParensEmptyWord()
        {
            var parser = BuildParenParser();
            string parens = string.Empty;

            var tree = parser.Parse(GetParenTokens(parens), null);

            VerifyParenParseTree(parens, tree);
        }

        [TestMethod]
        public void TestParensSinglePair()
        {
            var parser = BuildParenParser();
            string parens = "()";

            var tree = parser.Parse(GetParenTokens(parens), null);

            VerifyParenParseTree(parens, tree);
        }

        [TestMethod]
        public void TestParensNested()
        {
            var parser = BuildParenParser();
            string parens = "(())()";

            var tree = parser.Parse(GetParenTokens(parens), null);

            VerifyParenParseTree(parens, tree);
        }

        [TestMethod]
        public void TestParensBig()
        {
            var parser = BuildParenParser();
            string parens = "((((((()))()(()))(())(()(()))()(())()()(()))(()(())((()))(()(()))(())(()))(())()()((())(())()()(()))(())()(()(()))()()(()))((()(()(()))(())((())()()()(()))(())((()))()(())(()))(((()))()(())()(()))((()))((()))(())(()(()))()()(()))((((()))()()(()))(()(()))(()(()))()(())(()))(((())((())()(()))((())()(()))()(())()(()))((())(()))(()((()))()()(()))()((())()()(()))((()))(())()(()))(((())()(()))((()))(()(())(()))(())()()(()))((()()()(()))(())(())(())(()))(())((())()()()(()))((())(()))(())()((()))()(()))";

            var tree = parser.Parse(GetParenTokens(parens), null);

            VerifyParenParseTree(parens, tree);
        }

        [TestMethod]
        public void TestParensUnmatched()
        {
            var parser = BuildParenParser();
            string parens = "(()";

            var diag = new Mock<IDiagnostics>();
            Assert.ThrowsException<ParseException>(() => parser.Parse(GetParenTokens(parens), diag.Object));
            MockDiagnostics.Verify(diag, "UnexpectedSymbol");
        }

        private static IEnumerable<Token<ParenAlphabet>> GetParenTokens(string s)
        {
            foreach (char c in s)
            {
                if (c == '(')
                {
                    yield return new Token<ParenAlphabet> { Category = ParenAlphabet.L };
                }
                else
                {
                    yield return new Token<ParenAlphabet> { Category = ParenAlphabet.R };
                }
            }

            yield return new Token<ParenAlphabet> { Category = ParenAlphabet.EOF };
        }

        private static void VerifyParenParseTree(string parens, ParseTree<ParenAlphabet> tree)
        {
            Assert.IsInstanceOfType(tree, typeof(Brunch<ParenAlphabet>));
            Assert.AreEqual(ParenAlphabet.S, tree.Category);
            var root = tree as Brunch<ParenAlphabet>;
            if (parens.Length == 0)
            {
                Assert.AreEqual(0, root.Children.Count);
                return;
            }

            int depth = 0;
            int last = 0;
            int child_idx = 0;
            for (int i = 0; i < parens.Length; i++)
            {
                if (parens[i] == '(')
                {
                    depth++;
                }
                else
                {
                    depth--;
                }

                if (depth == 0)
                {
                    Assert.IsTrue(root.Children.Count >= child_idx + 3);
                    Assert.IsInstanceOfType(root.Children[child_idx], typeof(Token<ParenAlphabet>));
                    Assert.IsInstanceOfType(root.Children[child_idx + 1], typeof(Brunch<ParenAlphabet>));
                    Assert.IsInstanceOfType(root.Children[child_idx + 2], typeof(Token<ParenAlphabet>));
                    Assert.AreEqual(ParenAlphabet.L, root.Children[child_idx].Category);
                    Assert.AreEqual(ParenAlphabet.S, root.Children[child_idx + 1].Category);
                    Assert.AreEqual(ParenAlphabet.R, root.Children[child_idx + 2].Category);

                    VerifyParenParseTree(parens.Substring(last + 1, i - last - 1), root.Children[child_idx + 1] as Brunch<ParenAlphabet>);

                    last = i + 1;
                    child_idx += 3;
                }
            }

            Assert.AreEqual(child_idx, root.Children.Count);
        }

        private static Parser<ParenAlphabet> BuildParenParser()
        {
            var grammar = new Grammar<ParenAlphabet>
            {
                StartSymbol = ParenAlphabet.S,
                Rules = new ReadOnlyCollection<Rule<ParenAlphabet>>(new List<Rule<ParenAlphabet>>
                {
                    new Rule<ParenAlphabet>
                    {
                        Name = "paren",
                        Lhs = ParenAlphabet.S,
                        Rhs = Concat<ParenAlphabet>(
                            ParenAlphabet.L.ToRegex(),
                            ParenAlphabet.S.ToRegex(),
                            ParenAlphabet.R.ToRegex()).Starred()
                    }
                })
            };
            return ParserFactory<ParenAlphabet>.MakeParser(grammar, ParenAlphabet.EOF);
        }
    }
}
