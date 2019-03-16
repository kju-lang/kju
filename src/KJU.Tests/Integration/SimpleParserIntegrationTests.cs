namespace KJU.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class SimpleParserIntegrationTests
    {
        private enum ParenAlphabet
        {
            L,
            R,
            S
        }

        [TestMethod]
        [Ignore]
        public void TestParensEmptyWord()
        {
            var parser = BuildParenParser();
            var tree = parser.Parse(new Token<ParenAlphabet>[] { });

            // What happens now?
        }

        [TestMethod]
        [Ignore]
        public void TestParensSinglePair()
        {
            var parser = BuildParenParser();
            var tree = parser.Parse(new Token<ParenAlphabet>[]
            {
                new Token<ParenAlphabet>
                {
                    Category = ParenAlphabet.L
                },
                new Token<ParenAlphabet>
                {
                    Category = ParenAlphabet.R
                },
            });

            Assert.IsInstanceOfType(tree, typeof(Brunch<ParenAlphabet>));
            var root = tree as Brunch<ParenAlphabet>;
            Assert.AreEqual(ParenAlphabet.S, root.Category);
            var children = root.Children;
            Assert.AreEqual(2, children.Count);
            Assert.AreEqual(ParenAlphabet.L, children[0].Category);
            Assert.AreEqual(ParenAlphabet.R, children[1].Category);
            Assert.IsNotInstanceOfType(children[0], typeof(Brunch<ParenAlphabet>));
            Assert.IsNotInstanceOfType(children[1], typeof(Brunch<ParenAlphabet>));
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
                        Lhs = ParenAlphabet.S,
                        Rhs = Concat<ParenAlphabet>(
                            ParenAlphabet.L.ToRegex(),
                            ParenAlphabet.S.ToRegex(),
                            ParenAlphabet.R.ToRegex()).Starred()
                    }
                })
            };
            var compiledGrammar = GrammarCompiler<ParenAlphabet>.CompileGrammar(grammar);
            var nullables = NullablesHelper<ParenAlphabet>.GetNullableSymbols(compiledGrammar);
            var first = FirstHelper<ParenAlphabet>.GetFirstSymbols(compiledGrammar, nullables);
            var firstInversed = InverseRelationHelper<DfaAndState<ParenAlphabet>, ParenAlphabet>.InverseRelation(first);
            var follow = FollowHelper<ParenAlphabet>.GetFollowSymbols(compiledGrammar, nullables, firstInversed);
            var firstPlus = FirstPlusHelper<ParenAlphabet>.GetFirstPlusSymbols(firstInversed, follow, nullables);
            var table = ParseTableGenerator<ParenAlphabet>.Parse(compiledGrammar, follow, firstPlus);
            return new Parser<ParenAlphabet>(compiledGrammar, table);
        }
    }
}