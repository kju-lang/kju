namespace KJU.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using KJU.Core.Lexer;
    using KJU.Core.Parser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class ParserSmallGrammarTests
    {
        private enum Alphabet
        {
            S,
            T,
            U,
            X,
            Y,
            Z,
            EOF
        }

        [TestMethod]
        public void TestXYZParseTree()
        {
            var parser = ParserFactory<Alphabet>.MakeParser(GetGrammar(), Alphabet.EOF);
            var tokens = new List<Token<Alphabet>>
            {
                new Token<Alphabet> { Category = Alphabet.X },
                new Token<Alphabet> { Category = Alphabet.Y },
                new Token<Alphabet> { Category = Alphabet.Z },
                new Token<Alphabet> { Category = Alphabet.EOF },
            };

            var tree = parser.Parse(tokens);

            Assert.IsInstanceOfType(tree, typeof(Brunch<Alphabet>));
            var root = tree as Brunch<Alphabet>;
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(Alphabet.S, root.Category);
            Assert.IsInstanceOfType(root.Children[0], typeof(Token<Alphabet>));
            Assert.AreEqual(Alphabet.X, root.Children[0].Category);
            Assert.IsInstanceOfType(root.Children[1], typeof(Brunch<Alphabet>));
            var chld = root.Children[1] as Brunch<Alphabet>;
            Assert.AreEqual(Alphabet.U, chld.Category);
            Assert.AreEqual(2, chld.Children.Count);
            Assert.IsInstanceOfType(chld.Children[0], typeof(Token<Alphabet>));
            Assert.IsInstanceOfType(chld.Children[1], typeof(Token<Alphabet>));
            Assert.AreEqual(Alphabet.Y, chld.Children[0].Category);
            Assert.AreEqual(Alphabet.Z, chld.Children[1].Category);
        }

        [TestMethod]
        public void TestYXZParseTree()
        {
            var parser = ParserFactory<Alphabet>.MakeParser(GetGrammar(), Alphabet.EOF);
            var tokens = new List<Token<Alphabet>>
            {
                new Token<Alphabet> { Category = Alphabet.Y },
                new Token<Alphabet> { Category = Alphabet.X },
                new Token<Alphabet> { Category = Alphabet.Z },
                new Token<Alphabet> { Category = Alphabet.EOF },
            };

            var tree = parser.Parse(tokens);

            Assert.IsInstanceOfType(tree, typeof(Brunch<Alphabet>));
            var root = tree as Brunch<Alphabet>;
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(Alphabet.S, root.Category);
            Assert.IsInstanceOfType(root.Children[1], typeof(Token<Alphabet>));
            Assert.AreEqual(Alphabet.Z, root.Children[1].Category);
            Assert.IsInstanceOfType(root.Children[0], typeof(Brunch<Alphabet>));
            var chld = root.Children[0] as Brunch<Alphabet>;
            Assert.AreEqual(Alphabet.T, chld.Category);
            Assert.AreEqual(2, chld.Children.Count);
            Assert.IsInstanceOfType(chld.Children[0], typeof(Token<Alphabet>));
            Assert.IsInstanceOfType(chld.Children[1], typeof(Token<Alphabet>));
            Assert.AreEqual(Alphabet.Y, chld.Children[0].Category);
            Assert.AreEqual(Alphabet.X, chld.Children[1].Category);
        }

        private static Grammar<Alphabet> GetGrammar()
        {
            return new Grammar<Alphabet>
            {
                StartSymbol = Alphabet.S,
                Rules = new ReadOnlyCollection<Rule<Alphabet>>(new List<Rule<Alphabet>>
                {
                    new Rule<Alphabet>
                    {
                        Name = "right-leaning (XYZ)",
                        Lhs = Alphabet.S,
                        Rhs = Concat<Alphabet>(
                            Alphabet.X.ToRegex(),
                            Alphabet.U.ToRegex())
                    },
                    new Rule<Alphabet>
                    {
                        Name = "left-leaning (YXZ)",
                        Lhs = Alphabet.S,
                        Rhs = Concat<Alphabet>(
                            Alphabet.T.ToRegex(),
                            Alphabet.Z.ToRegex())
                    },
                    new Rule<Alphabet>
                    {
                        Name = "U-production",
                        Lhs = Alphabet.U,
                        Rhs = Concat<Alphabet>(
                            Alphabet.Y.ToRegex(),
                            Alphabet.Z.ToRegex())
                    },
                    new Rule<Alphabet>
                    {
                        Name = "T-production",
                        Lhs = Alphabet.T,
                        Rhs = Concat<Alphabet>(
                            Alphabet.Y.ToRegex(),
                            Alphabet.X.ToRegex())
                    }
                })
            };
        }
    }
}