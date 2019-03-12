namespace KJU.Tests.Lexer
{
   using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

     public enum DummyTokens
    {
        T_A, T_B, T_C
    }

    [TestClass]
    public class LexerTests
    {
        private List<IState> states;

        [TestInitialize]
        public void Setup()
        {
            this.states = new List<IState>();
            for (int i = 0; i < 30; ++i)
            {
                IState s = new Mock<IState>().Object;
                this.states.Add(s);
            }
        }

        [TestMethod]
        public void Test0Empty()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['\uffff'] = this.states[0];
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.IsStable(this.states[0])).Returns(true);
            var ret = lexer.Scan(StringToLetters(string.Empty)).ToList();
            Assert.AreEqual(0, ret.Count());
        }

        [TestMethod]
        public void Test1OneToken()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.T_A);
            var ret = lexer.Scan(StringToLetters("a")).ToList();
            Assert.AreEqual(1, ret.Count());
            Assert.AreEqual(DummyTokens.T_A, ret[0].Category);
            Assert.AreEqual("a", ret[0].Text);
            Assert.AreEqual(0, ((Location)ret[0].InputRange.Begin).X);
            Assert.AreEqual(1, ((Location)ret[0].InputRange.End).X);
        }

        [TestMethod]
        public void Test2NoTokensNonempty()
        {
                var dfa = new Mock<IDfa<DummyTokens?, char>>();
                Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
                Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
                trans0['a'] = this.states[0];
                trans0['\uffff'] = this.states[0];
                dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
                dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
                dfa.Setup(x => x.IsStable(this.states[0])).Returns(true);
            try
            {
                lexer.Scan(StringToLetters("a")).ToList();
                Assert.Fail();
            }
            catch (FormatException)
            {
            }
        }

        [TestMethod]
        public void Test3TwoTokens()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[2];
            trans1['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.T_A);
            var ret = lexer.Scan(StringToLetters("aa")).ToList();
            Assert.AreEqual(2, ret.Count());
            Assert.AreEqual(DummyTokens.T_A, ret[0].Category);
            Assert.AreEqual("a", ret[0].Text);
            Assert.AreEqual(0, ((Location)ret[0].InputRange.Begin).X);
            Assert.AreEqual(1, ((Location)ret[0].InputRange.End).X);
            Assert.AreEqual(DummyTokens.T_A, ret[1].Category);
            Assert.AreEqual("a", ret[1].Text);
            Assert.AreEqual(1, ((Location)ret[1].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)ret[1].InputRange.End).X);
        }

        [TestMethod]
        public void Test4TwoDifferentTokens()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['b'] = this.states[3];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[2];
            trans1['b'] = this.states[2];
            trans1['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans1);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.T_A);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.T_B);
            var ret = lexer.Scan(StringToLetters("ab")).ToList();
            Assert.AreEqual(2, ret.Count());
            Assert.AreEqual(DummyTokens.T_A, ret[0].Category);
            Assert.AreEqual("a", ret[0].Text);
            Assert.AreEqual(0, ((Location)ret[0].InputRange.Begin).X);
            Assert.AreEqual(1, ((Location)ret[0].InputRange.End).X);
            Assert.AreEqual(DummyTokens.T_B, ret[1].Category);
            Assert.AreEqual("b", ret[1].Text);
            Assert.AreEqual(1, ((Location)ret[1].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)ret[1].InputRange.End).X);
        }

        [TestMethod]
        public void Test5TokenNontoken()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['b'] = this.states[2];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[2];
            trans1['b'] = this.states[2];
            trans1['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.Transitions(this.states[2])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.T_A);
            try
            {
                lexer.Scan(StringToLetters("ab")).ToList();
                Assert.Fail();
            }
            catch (FormatException)
            {
            }
        }

[TestMethod]
        public void Test6LongToken()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['b'] = this.states[2];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[2];
            trans1['b'] = this.states[3];
            trans1['\uffff'] = this.states[2];
            Dictionary<char, IState> trans3 = new Dictionary<char, IState>();
            trans3['a'] = this.states[2];
            trans3['b'] = this.states[3];
            trans3['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);

            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.T_A);
            var ret = lexer.Scan(StringToLetters("ab")).ToList();
            Assert.AreEqual(1, ret.Count());
            Assert.AreEqual(DummyTokens.T_A, ret[0].Category);
            Assert.AreEqual("ab", ret[0].Text);
            Assert.AreEqual(0, ((Location)ret[0].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)ret[0].InputRange.End).X);
        }

        [TestMethod]
        public void Test7LongTokenInterrupted()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['b'] = this.states[2];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[2];
            trans1['b'] = this.states[3];
            trans1['\uffff'] = this.states[2];
            Dictionary<char, IState> trans3 = new Dictionary<char, IState>();
            trans3['a'] = this.states[2];
            trans3['b'] = this.states[3];
            trans3['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);

            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.T_A);
            try
            {
                lexer.Scan(StringToLetters("aa")).ToList();
                Assert.Fail();
            }
            catch (FormatException)
            {
            }
        }

        [TestMethod]
        public void Test8Greedy()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[3];
            trans1['\uffff'] = this.states[2];
            Dictionary<char, IState> trans3 = new Dictionary<char, IState>();
            trans3['a'] = this.states[2];
            trans3['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.T_B);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.T_A);
                var ret = lexer.Scan(StringToLetters("aa")).ToList();
                Assert.AreEqual(1, ret.Count());
                Assert.AreEqual(DummyTokens.T_A, ret[0].Category);
                Assert.AreEqual("aa", ret[0].Text);
                Assert.AreEqual(0, ((Location)ret[0].InputRange.Begin).X);
                Assert.AreEqual(2, ((Location)ret[0].InputRange.End).X);
        }

        [TestMethod]
        public void Test9GreedySub()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[3];
            trans1['\uffff'] = this.states[2];
            Dictionary<char, IState> trans3 = new Dictionary<char, IState>();
            trans3['a'] = this.states[2];
            trans3['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.T_B);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.T_A);
            var ret = lexer.Scan(StringToLetters("a")).ToList();
            Assert.AreEqual(1, ret.Count());
            Assert.AreEqual(DummyTokens.T_B, ret[0].Category);
            Assert.AreEqual("a", ret[0].Text);
            Assert.AreEqual(0, ((Location)ret[0].InputRange.Begin).X);
            Assert.AreEqual(1, ((Location)ret[0].InputRange.End).X);
        }

        [TestMethod]
        public void TestAGreedyTwo()
        {
            var dfa = new Mock<IDfa<DummyTokens?, char>>();
            Lexer<DummyTokens?> lexer = new Lexer<DummyTokens?>(dfa.Object);
            dfa.Setup(x => x.StartingState()).Returns(this.states[0]);
            Dictionary<char, IState> trans0 = new Dictionary<char, IState>();
            trans0['a'] = this.states[1];
            trans0['\uffff'] = this.states[2];
            Dictionary<char, IState> trans1 = new Dictionary<char, IState>();
            trans1['a'] = this.states[3];
            trans1['\uffff'] = this.states[2];
            Dictionary<char, IState> trans3 = new Dictionary<char, IState>();
            trans3['a'] = this.states[2];
            trans3['\uffff'] = this.states[2];

            dfa.Setup(x => x.Transitions(this.states[0])).Returns(trans0);
            dfa.Setup(x => x.Transitions(this.states[1])).Returns(trans1);
            dfa.Setup(x => x.IsStable(this.states[2])).Returns(true);
            dfa.Setup(x => x.Transitions(this.states[3])).Returns(trans3);
            dfa.Setup(x => x.Label(this.states[1])).Returns(DummyTokens.T_B);
            dfa.Setup(x => x.Label(this.states[3])).Returns(DummyTokens.T_A);
            var ret = lexer.Scan(StringToLetters("aaa")).ToList();
            Assert.AreEqual(2, ret.Count());
            Assert.AreEqual(DummyTokens.T_A, ret[0].Category);
            Assert.AreEqual("aa", ret[0].Text);
            Assert.AreEqual(0, ((Location)ret[0].InputRange.Begin).X);
            Assert.AreEqual(2, ((Location)ret[0].InputRange.End).X);
            Assert.AreEqual(DummyTokens.T_B, ret[1].Category);
            Assert.AreEqual("a", ret[1].Text);
            Assert.AreEqual(2, ((Location)ret[1].InputRange.Begin).X);
            Assert.AreEqual(3, ((Location)ret[1].InputRange.End).X);
        }

        private static IEnumerable<KeyValuePair<ILocation, char>> StringToLetters(string input)
        {
            for (int i = 0; i != input.Length; ++i)
            {
                yield return new KeyValuePair<ILocation, char>(new Location { X = i }, input[i]);
            }

            yield return new KeyValuePair<ILocation, char>(new Location { X = input.Length }, (char)0xffff);
        }

        internal struct Location : ILocation
        {
            public int X;
        }
    }
}