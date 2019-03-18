namespace KJU.Tests.Regex
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Regex;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Util;
    using static KJU.Core.Regex.RegexUtils;

    [TestClass]
    public class RegexToNfaConverterTests
    {
        [TestMethod]
        public void TestAtomic()
        {
            var nfaAtomic = RegexToNfaConverter<char>.Convert('a'.ToRegex());
            var vocabulary = new List<char> { 'a', 'b' };

            var expected = new List<string> { "a" };
            var actual = GetAllAcceptedStringsUpToLength(3, vocabulary, nfaAtomic);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestConcat()
        {
            var regexConcat = Concat('x'.ToRegex(), 'y'.ToRegex());
            var nfaConcat = RegexToNfaConverter<char>.Convert(regexConcat);
            var vocabulary = new List<char>() { 'x', 'y', 'z' };

            var expected = new List<string>() { "xy" };
            var actual = GetAllAcceptedStringsUpToLength(4, vocabulary, nfaConcat);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestEmpty()
        {
            var nfaEmpty = RegexToNfaConverter<char>.Convert(new EmptyRegex<char>());
            var vocabulary = new List<char> { 'p', 'q' };

            var expected = new List<string>();
            var actual = GetAllAcceptedStringsUpToLength(2, vocabulary, nfaEmpty);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestEpsilon()
        {
            var nfaEpsilon = RegexToNfaConverter<char>.Convert(new EpsilonRegex<char>());
            var vocabulary = new List<char> { 'a', 'e' };

            var expected = new List<string> { string.Empty };
            var actual = GetAllAcceptedStringsUpToLength(2, vocabulary, nfaEpsilon);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStar()
        {
            var nfaStar = RegexToNfaConverter<char>.Convert('b'.ToRegex().Starred());
            var vocabulary = new List<char> { 'a', 'b' };

            var expected = new List<string>() { string.Empty, "b", "bb", "bbb", "bbbb" };
            var actual = GetAllAcceptedStringsUpToLength(4, vocabulary, nfaStar);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSum()
        {
            var regexSum = Sum('x'.ToRegex(), 'y'.ToRegex());
            var nfaStar = RegexToNfaConverter<char>.Convert(regexSum);
            var vocabulary = new List<char>() { 'x', 'y', 'z' };
            var expected = new List<string> { "x", "y" };
            var actual = GetAllAcceptedStringsUpToLength(3, vocabulary, nfaStar);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestComplex()
        {
            // equivalent to: a*b | bc* | bad
            var regex = Sum(
                Concat('a'.ToRegex().Starred(), 'b'.ToRegex()),
                Concat('b'.ToRegex(), 'c'.ToRegex().Starred(), Sum(new EmptyRegex<char>(), new EpsilonRegex<char>())),
                Concat('b'.ToRegex(), 'a'.ToRegex(), 'd'.ToRegex()));

            var nfa = RegexToNfaConverter<char>.Convert(regex);
            var vocabulary = new List<char>() { 'a', 'b', 'c', 'd' };

            var actual = GetAllAcceptedStringsUpToLength(5, vocabulary, nfa);
            var expected = new List<string>()
                { "b", "ab", "bc", "aab", "bad", "bcc", "aaab", "bccc", "aaaab", "bcccc" };
            var expectedText = string.Join(", ", expected);
            var actualText = string.Join(", ", actual);
            CollectionAssert.AreEqual(expected, actual, $"Expected: {expectedText}, actual: {actualText}");
        }

        [TestMethod]
        public void TestHashSet()
        {
            var regex = Sum('x'.ToRegex(), 'y'.ToRegex(), 'z'.ToRegex(), 't'.ToRegex());

            var nfa = RegexToNfaConverter<char>.Convert(regex);
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

        private static List<string> GetAllAcceptedStringsUpToLength(int maxLength, List<char> vocabulary, INfa<char> nfa)
        {
            return GetAllStringsUpToLength(maxLength, vocabulary).Where(nfa.Accepts).ToList();
        }

        private static IEnumerable<string> GetAllStringsUpToLength(int maxLength, List<char> vocabulary)
        {
            var result = new List<List<string>>() { new List<string>() { string.Empty } };

            for (var length = 0; length < maxLength; length++)
            {
                result.Add(new List<string>());
                foreach (var str in result[length])
                {
                    foreach (var lastCharacter in vocabulary)
                    {
                        result[length + 1].Add(str + lastCharacter);
                    }
                }
            }

            return result.SelectMany(x => x).ToList();
        }
    }
}