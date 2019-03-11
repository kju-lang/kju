namespace KJU.Tests.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core;
    using KJU.Core.Automata;
    using KJU.Core.Regex;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegexToNfaConverterTests
    {
        [TestMethod]
        public void TestAtomic()
        {
            var nfaAtomic = RegexToNfaConverter.Convert(new AtomicRegex('a'));
            var vocabulary = new List<char>() { 'a', 'b' };
            var accepted = this.GetAllAcceptedStringsUpToLength(3, vocabulary, nfaAtomic);

            Assert.IsTrue(accepted.SequenceEqual(new List<string>() { "a" }));
        }

        [TestMethod]
        public void TestConcat()
        {
            var regexConcat = new ConcatRegex(new AtomicRegex('x'), new AtomicRegex('y'));
            var nfaConcat = RegexToNfaConverter.Convert(regexConcat);
            var vocabulary = new List<char>() { 'x', 'y', 'z' };
            var accepted = this.GetAllAcceptedStringsUpToLength(4, vocabulary, nfaConcat);

            Assert.IsTrue(accepted.SequenceEqual(new List<string>() { "xy" }));
        }

        [TestMethod]
        public void TestEmpty()
        {
            var nfaEmpty = RegexToNfaConverter.Convert(new EmptyRegex());
            var vocabulary = new List<char>() { 'p', 'q' };
            var accepted = this.GetAllAcceptedStringsUpToLength(2, vocabulary, nfaEmpty);

            Assert.IsTrue(accepted.SequenceEqual(new List<string>() { }));
        }

        [TestMethod]
        public void TestEpsilon()
        {
            var nfaEpsilon = RegexToNfaConverter.Convert(new EpsilonRegex());
            var vocabulary = new List<char>() { 'a', 'e' };
            var accepted = this.GetAllAcceptedStringsUpToLength(2, vocabulary, nfaEpsilon);

            Assert.IsTrue(accepted.SequenceEqual(new List<string>() { string.Empty }));
        }

        [TestMethod]
        public void TestStar()
        {
            var nfaStar = RegexToNfaConverter.Convert(new StarRegex(new AtomicRegex('b')));
            var vocabulary = new List<char>() { 'a', 'b' };
            var accepted = this.GetAllAcceptedStringsUpToLength(4, vocabulary, nfaStar);

            Assert.IsTrue(accepted.SequenceEqual(new List<string>() { string.Empty, "b", "bb", "bbb", "bbbb" }));
        }

        [TestMethod]
        public void TestSum()
        {
            var regexSum = new SumRegex(new AtomicRegex('x'), new AtomicRegex('y'));
            var nfaStar = RegexToNfaConverter.Convert(regexSum);
            var vocabulary = new List<char>() { 'x', 'y', 'z' };
            var accepted = this.GetAllAcceptedStringsUpToLength(3, vocabulary, nfaStar);

            Assert.IsTrue(accepted.SequenceEqual(new List<string>() { "x", "y" }));
        }

        [TestMethod]
        public void TestComplex()
        {
            // equivalent to: a*b | bc* | bad
            var regex = new SumRegex(
                new ConcatRegex(
                    new SumRegex(
                        new ConcatRegex(new StarRegex(new AtomicRegex('a')), new AtomicRegex('b')),
                        new ConcatRegex(new AtomicRegex('b'), new StarRegex(new AtomicRegex('c')))),
                    new SumRegex(new EmptyRegex(), new EpsilonRegex())),
                new ConcatRegex(new AtomicRegex('b'), new ConcatRegex(new AtomicRegex('a'), new AtomicRegex('d'))));

            var nfa = RegexToNfaConverter.Convert(regex);
            var vocabulary = new List<char>() { 'a', 'b', 'c', 'd' };

            var accepted = this.GetAllAcceptedStringsUpToLength(5, vocabulary, nfa);
            var expected = new List<string>() { "b", "ab", "bc", "aab", "bad", "bcc", "aaab", "bccc", "aaaab", "bcccc" };

            Assert.IsTrue(accepted.SequenceEqual(expected));
        }

        [TestMethod]
        public void TestHashSet()
        {
            var regex = new SumRegex(
                new SumRegex(new SumRegex(new AtomicRegex('x'), new AtomicRegex('y')), new AtomicRegex('z')), new AtomicRegex('t'));

            var nfa = RegexToNfaConverter.Convert(regex);
            var returned = new HashSet<IState>();

            for (int tries = 1; tries <= 10; tries++)
            {
                var epsReachable = new HashSet<IState>() { nfa.StartingState() };
                for (int depth = 1; depth <= 20; depth++)
                {
                    var toAdd = new HashSet<IState>();
                    foreach (var state in epsReachable)
                    {
                        foreach (var nextState in nfa.EpsilonTransitions(state))
                        {
                            if (!epsReachable.Contains(nextState))
                            {
                                toAdd.Add(nextState);
                            }
                        }
                    }

                    foreach (var state in toAdd)
                    {
                        epsReachable.Add(state);
                    }

                    // the size of this set should be a small constant (at most the total number of states)
                    Assert.IsTrue(epsReachable.Count < 10);
                }

                foreach (IState start in epsReachable)
                {
                    var transitions = nfa.Transitions(start);
                    if (transitions.ContainsKey('x'))
                    {
                        foreach (IState state in transitions['x'])
                        {
                            returned.Add(state);
                        }
                    }
                }
            }

            // all tries should consistently return the same single state corresponding to just having read the letter x
            Assert.AreEqual(returned.Count, 1);
        }

        private List<string> GetAllAcceptedStringsUpToLength(int maxLength, List<char> vocabulary, INfa nfa)
        {
            var result = new List<string>();
            foreach (string input in this.GetAllStringsUpToLength(maxLength, vocabulary))
            {
                if (NfaAcceptance.Accepts(nfa, input))
                {
                    result.Add(input);
                }
            }

            return result;
        }

        private List<string> GetAllStringsUpToLength(int maxLength, List<char> vocabulary)
        {
            var result = new List<List<string>>() { new List<string>() { string.Empty } };

            for (int length = 0; length < maxLength; length++)
            {
                result.Add(new List<string>());
                foreach (string str in result[length])
                {
                    foreach (char lastCharacter in vocabulary)
                    {
                        result[length + 1].Add(str + lastCharacter);
                    }
                }
            }

            return result.SelectMany(x => x).ToList();
        }
    }
}