namespace KJU.Tests.Lexer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Diagnostics;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static Core.Constants;

    public enum DummyTokens
    {
        A,
        B,
        Eof
    }

    [TestClass]
    public class LexerTests
    {
        private readonly List<IState> states = Enumerable.Range(0, 30).Select(x => new Mock<IState>().Object).ToList();

        [TestMethod]
        public void Test0Empty()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            var trans0 = new Dictionary<char, IState> { [EndOfInput] = this.states[0] };
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.IsStable(this.states[0])).Returns(true);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            var input = StringToLetters(string.Empty);
            var ret = lexer.Scan(input, null).ToList();
            var expected = 1;
            var actual = ret.Count;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test1OneToken()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState> { ['a'] = this.states[1], [EndOfInput] = this.states[2] };
            var trans1 = new Dictionary<char, IState> { [EndOfInput] = this.states[2] };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.A);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            var result = lexer.Scan(StringToLetters("a"), null).ToList();
            var inputRange = result[0].InputRange;
            var beginLocation = (Location)inputRange.Begin;
            var endLocation = (Location)inputRange.End;
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(DummyTokens.A, result[0].Category);
            Assert.AreEqual("a", result[0].Text);
            Assert.AreEqual(0, beginLocation.X);
            Assert.AreEqual(1, endLocation.X);
        }

        [TestMethod]
        public void Test2NoTokensNonempty()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            var trans0 = new Dictionary<char, IState> { ['a'] = this.states[0], [EndOfInput] = this.states[0] };
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.IsStable(this.states[0])).Returns(true);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            var diag = new Mock<IDiagnostics>();
            Assert.ThrowsException<LexerException>(() => lexer.Scan(StringToLetters("a"), diag.Object).ToList());
            MockDiagnostics.Verify(diag, Lexer<DummyTokens?>.NonTokenDiagnostic);
        }

        [TestMethod]
        public void Test3TwoTokens()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState> { ['a'] = this.states[1], [EndOfInput] = this.states[2] };
            var trans1 = new Dictionary<char, IState> { ['a'] = this.states[2], [EndOfInput] = this.states[2] };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.A);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            var result = lexer.Scan(StringToLetters("aa"), null).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(DummyTokens.A, result[0].Category);
            Assert.AreEqual("a", result[0].Text);
            Assert.AreEqual(0, ((Location)result[0].InputRange.Begin).X);
            Assert.AreEqual(1, ((Location)result[0].InputRange.End).X);
            Assert.AreEqual(DummyTokens.A, result[1].Category);
            Assert.AreEqual("a", result[1].Text);
            Assert.AreEqual(1, ((Location)result[1].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)result[1].InputRange.End).X);
        }

        [TestMethod]
        public void Test4TwoDifferentTokens()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState>
            {
                ['a'] = this.states[1], ['b'] = this.states[3], [EndOfInput] = this.states[2]
            };
            var trans1 = new Dictionary<char, IState>
            {
                ['a'] = this.states[2], ['b'] = this.states[2], [EndOfInput] = this.states[2]
            };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans1);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.A);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.B);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            var result = lexer.Scan(StringToLetters("ab"), null).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(DummyTokens.A, result[0].Category);
            Assert.AreEqual("a", result[0].Text);
            Assert.AreEqual(0, ((Location)result[0].InputRange.Begin).X);
            Assert.AreEqual(1, ((Location)result[0].InputRange.End).X);
            Assert.AreEqual(DummyTokens.B, result[1].Category);
            Assert.AreEqual("b", result[1].Text);
            Assert.AreEqual(1, ((Location)result[1].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)result[1].InputRange.End).X);
        }

        [TestMethod]
        public void Test5TokenNotToken()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState>
            {
                ['a'] = this.states[1], ['b'] = this.states[2], [EndOfInput] = this.states[2]
            };
            var trans1 = new Dictionary<char, IState>
            {
                ['a'] = this.states[2], ['b'] = this.states[2], [EndOfInput] = this.states[2]
            };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.Transitions(this.states[2])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.A);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            var diag = new Mock<IDiagnostics>();
            Assert.ThrowsException<LexerException>(() => lexer.Scan(StringToLetters("ab"), diag.Object).ToList());
            MockDiagnostics.Verify(diag, Lexer<DummyTokens?>.NonTokenDiagnostic);
        }

        [TestMethod]
        public void Test6LongToken()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState>
            {
                ['a'] = this.states[1], ['b'] = this.states[2], [EndOfInput] = this.states[2]
            };
            var trans1 = new Dictionary<char, IState>
            {
                ['a'] = this.states[2], ['b'] = this.states[3], [EndOfInput] = this.states[2]
            };
            var trans3 = new Dictionary<char, IState>
            {
                ['a'] = this.states[2], ['b'] = this.states[3], [EndOfInput] = this.states[2]
            };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            foreach (char c in trans3.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[3], c)).Returns(trans3[c]);
            }

            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.A);
            var result = lexer.Scan(StringToLetters("ab"), null).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(DummyTokens.A, result[0].Category);
            Assert.AreEqual("ab", result[0].Text);
            Assert.AreEqual(0, ((Location)result[0].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)result[0].InputRange.End).X);
        }

        [TestMethod]
        public void Test7LongTokenInterrupted()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState>
            {
                ['a'] = this.states[1], ['b'] = this.states[2], [EndOfInput] = this.states[2]
            };
            var trans1 = new Dictionary<char, IState>
            {
                ['a'] = this.states[2], ['b'] = this.states[3], [EndOfInput] = this.states[2]
            };
            var trans3 = new Dictionary<char, IState>
            {
                ['a'] = this.states[2], ['b'] = this.states[3], [EndOfInput] = this.states[2]
            };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            foreach (char c in trans3.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[3], c)).Returns(trans3[c]);
            }

            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.A);

            var diag = new Mock<IDiagnostics>();
            Assert.ThrowsException<LexerException>(() => lexer.Scan(StringToLetters("aa"), diag.Object).ToList());
            MockDiagnostics.Verify(diag, Lexer<DummyTokens?>.NonTokenDiagnostic);
        }

        [TestMethod]
        public void Test8Greedy()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState> { ['a'] = this.states[1], [EndOfInput] = this.states[2] };
            var trans1 = new Dictionary<char, IState> { ['a'] = this.states[3], [EndOfInput] = this.states[2] };
            var trans3 = new Dictionary<char, IState> { ['a'] = this.states[2], [EndOfInput] = this.states[2] };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.B);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.A);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            foreach (char c in trans3.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[3], c)).Returns(trans3[c]);
            }

            var result = lexer.Scan(StringToLetters("aa"), null).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(DummyTokens.A, result[0].Category);
            Assert.AreEqual("aa", result[0].Text);
            Assert.AreEqual(0, ((Location)result[0].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)result[0].InputRange.End).X);
        }

        [TestMethod]
        public void Test9GreedySub()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            var trans0 = new Dictionary<char, IState> { ['a'] = this.states[1], [EndOfInput] = this.states[2] };
            var trans1 = new Dictionary<char, IState> { ['a'] = this.states[3], [EndOfInput] = this.states[2] };
            var trans3 = new Dictionary<char, IState> { ['a'] = this.states[2], [EndOfInput] = this.states[2] };

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.B);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.A);
            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            foreach (char c in trans3.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[3], c)).Returns(trans3[c]);
            }

            var result = lexer.Scan(StringToLetters("a"), null).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(DummyTokens.B, result[0].Category);
            Assert.AreEqual("a", result[0].Text);
            Assert.AreEqual(0, ((Location)result[0].InputRange.Begin).X);
            Assert.AreEqual(1, ((Location)result[0].InputRange.End).X);
        }

        [TestMethod]
        public void TestAGreedyTwo()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            var lexer = new Lexer<DummyTokens?>(dfa.Object, DummyTokens.Eof);

            var trans0 = new Dictionary<char, IState> { ['a'] = this.states[1], [EndOfInput] = this.states[2] };
            var trans1 = new Dictionary<char, IState> { ['a'] = this.states[3], [EndOfInput] = this.states[2] };
            var trans3 = new Dictionary<char, IState> { ['a'] = this.states[2], [EndOfInput] = this.states[2] };

            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.B);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.A);

            foreach (char c in trans0.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[0], c)).Returns(trans0[c]);
            }

            foreach (char c in trans1.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[1], c)).Returns(trans1[c]);
            }

            foreach (char c in trans3.Keys)
            {
                dfa.Setup(x => x.Transition(this.states[3], c)).Returns(trans3[c]);
            }

            var result = lexer.Scan(StringToLetters("aaa"), null).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(DummyTokens.A, result[0].Category);
            Assert.AreEqual("aa", result[0].Text);
            Assert.AreEqual(0, ((Location)result[0].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)result[0].InputRange.End).X);
            Assert.AreEqual(DummyTokens.B, result[1].Category);
            Assert.AreEqual("a", result[1].Text);
            Assert.AreEqual(2, ((Location)result[1].InputRange.Begin).X);
            Assert.AreEqual(3, ((Location)result[1].InputRange.End).X);
        }

        private static IEnumerable<KeyValuePair<ILocation, char>> StringToLetters(string input)
        {
            return input.Select((letter, i) => new KeyValuePair<ILocation, char>(new Location { X = i }, letter))
                .Append(new KeyValuePair<ILocation, char>(new Location { X = input.Length }, (char)0xffff));
        }

        private struct Location : ILocation
        {
            public int X;
        }
    }
}
