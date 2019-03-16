namespace KJU.Tests.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Parser;
    using KJU.Core.Regex;
    using KJU.Core.Util;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
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
        public void SimpleParenTest()
        {
            var grammar = new Grammar<ParenAlphabet>();
            grammar.Rules = new List<Rule<ParenAlphabet>>() {
                new Rule<ParenAlphabet> { Name = "paren", Lhs = ParenAlphabet.Expr, Rhs = Concat<ParenAlphabet>(
                            ParenAlphabet.L.ToRegex(),
                            ParenAlphabet.Expr.ToRegex(),
                            ParenAlphabet.R.ToRegex()) },
                new Rule<ParenAlphabet> { Name = "X", Lhs = ParenAlphabet.Expr, Rhs = ParenAlphabet.X.ToRegex() }
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
        public void HarderParenTest()
        {
            var grammar = new Grammar<ParenAlphabet>();
            grammar.Rules = new List<Rule<ParenAlphabet>>() {
                new Rule<ParenAlphabet> { Name = "paren", Lhs = ParenAlphabet.Expr, Rhs = Concat<ParenAlphabet>(
                            ParenAlphabet.L.ToRegex(),
                            ParenAlphabet.Expr.ToRegex(),
                            ParenAlphabet.S.ToRegex(),
                            ParenAlphabet.R.ToRegex()) },
                new Rule<ParenAlphabet> { Name = "X", Lhs = ParenAlphabet.Expr, Rhs = ParenAlphabet.X.ToRegex() },
                new Rule<ParenAlphabet> { Name = "SY", Lhs = ParenAlphabet.S, Rhs = ParenAlphabet.Y.ToRegex().Optional() },
            };
            grammar.StartSymbol = ParenAlphabet.Expr;

            var compiledGrammar = GrammarCompiler<ParenAlphabet>.CompileGrammar(grammar);

            var follow = FollowHelper<ParenAlphabet>.GetFollowSymbols(compiledGrammar, null, null, ParenAlphabet.EOF);
            foreach (var c in follow)
            {
                Console.WriteLine("state: " + c.Key.Dfa.Label(c.Key.State) + " follow: " + string.Join(",", c.Value));
            }
        }
    }
}
