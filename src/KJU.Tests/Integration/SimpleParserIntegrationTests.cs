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
            S,
            EOF
        }

        [TestMethod]
        public void TestParensEmptyWord()
        {
            var parser = BuildParenParser();
            var tree = parser.Parse(new Token<ParenAlphabet>[] {
                new Token<ParenAlphabet> { Category = ParenAlphabet.EOF }
            });

            // What happens now?
        }

        [TestMethod]
        [Ignore]
        public void TestParensSinglePair()
        {
            var parser = BuildParenParser();
            var tree = parser.Parse(new Token<ParenAlphabet>[]
            {
                new Token<ParenAlphabet> { Category = ParenAlphabet.L },
                new Token<ParenAlphabet> { Category = ParenAlphabet.R },
                new Token<ParenAlphabet> { Category = ParenAlphabet.EOF }
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
                        Name = "paren",
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
            var firstInversed = first.InverseRelation();
            var follow = FollowHelper<ParenAlphabet>.GetFollowSymbols(compiledGrammar, nullables, firstInversed, ParenAlphabet.EOF);
            var followInversed = follow.InverseRelation();
            var firstPlus = FirstPlusHelper<ParenAlphabet>.GetFirstPlusSymbols(firstInversed, followInversed, nullables);
            var table = ParseTableGenerator<ParenAlphabet>.Parse(compiledGrammar, followInversed, firstPlus);
            return new Parser<ParenAlphabet>(compiledGrammar, table);
        }
    }
}