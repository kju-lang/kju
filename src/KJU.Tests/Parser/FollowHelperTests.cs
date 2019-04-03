namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Parser;
    using KJU.Core.Regex;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class FollowHelperTests
    {
        private enum ParenAlphabet
        {
            L,
            R,
            X,
            Y,
            S,
            Expr,
            EOF
        }

        [TestMethod]
        [Ignore]
        public void SimpleParenTest()
        {
            var grammar = new Grammar<ParenAlphabet>();
            var ruleParen =
                new Rule<ParenAlphabet>
                {
                    Name = "paren",
                    Lhs = ParenAlphabet.Expr,
                    Rhs = Concat<ParenAlphabet>(
                            ParenAlphabet.L.ToRegex(),
                            ParenAlphabet.Expr.ToRegex(),
                            ParenAlphabet.R.ToRegex())
                };
            var ruleX =
                new Rule<ParenAlphabet> { Name = "X", Lhs = ParenAlphabet.Expr, Rhs = ParenAlphabet.X.ToRegex() };
            grammar.Rules = new List<Rule<ParenAlphabet>>() {
                ruleParen,
                ruleX
            };
            grammar.StartSymbol = ParenAlphabet.Expr;

            var compiledGrammar = GrammarCompiler<ParenAlphabet>.CompileGrammar(grammar);

            var follow = FollowHelper<ParenAlphabet>.GetFollowSymbols(compiledGrammar, null, null, ParenAlphabet.EOF);
            foreach (var c in follow)
            {
                Console.WriteLine("state: " + c.Key.Dfa.Label(c.Key.State) + " follow: " + string.Join(",", c.Value));
            }
        }

        [TestMethod]
        [Ignore]
        public void HarderParenTest()
        {
            var grammar = new Grammar<ParenAlphabet>
            {
                Rules = new List<Rule<ParenAlphabet>>()
                {
                    new Rule<ParenAlphabet>
                    {
                        Name = "paren",
                        Lhs = ParenAlphabet.Expr,
                        Rhs = Concat(
                            ParenAlphabet.L.ToRegex(),
                            ParenAlphabet.Expr.ToRegex(),
                            ParenAlphabet.S.ToRegex(),
                            ParenAlphabet.R.ToRegex())
                    },
                    new Rule<ParenAlphabet> { Name = "X", Lhs = ParenAlphabet.Expr, Rhs = ParenAlphabet.X.ToRegex() },
                    new Rule<ParenAlphabet>
                    {
                        Name = "SY", Lhs = ParenAlphabet.S, Rhs = ParenAlphabet.Y.ToRegex().Optional()
                    },
                },
                StartSymbol = ParenAlphabet.Expr
            };

            var compiledGrammar = GrammarCompiler<ParenAlphabet>.CompileGrammar(grammar);

            var follow = FollowHelper<ParenAlphabet>.GetFollowSymbols(compiledGrammar, null, null, ParenAlphabet.EOF);
            foreach (var c in follow)
            {
                Console.WriteLine("state: " + c.Key.Dfa.Label(c.Key.State) + " follow: " + string.Join(",", c.Value));
            }
        }
    }
}
