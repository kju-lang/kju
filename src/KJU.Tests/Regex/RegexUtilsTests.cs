namespace KJU.Tests.Regex
{
    using KJU.Core.Regex;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegexUtilsTests
    {
        [TestMethod]
        public void TestToRegex()
        {
            var expected = new AtomicRegex<int>(123);
            var actual = 123.ToRegex();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSumEmpty()
        {
            var expected = new EmptyRegex<int>();
            var actual = RegexUtils.Sum<int>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSumNotEmpty()
        {
            var expected = new SumRegex<int>(new SumRegex<int>(1.ToRegex(), 2.ToRegex()), 3.ToRegex());
            var actual = RegexUtils.Sum(1.ToRegex(), 2.ToRegex(), 3.ToRegex());
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestConcatEmpty()
        {
            var expected = new EpsilonRegex<int>();
            var actual = RegexUtils.Concat<int>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestConcatNotEmpty()
        {
            var expected = new ConcatRegex<int>(new ConcatRegex<int>(1.ToRegex(), 2.ToRegex()), 3.ToRegex());
            var actual = RegexUtils.Concat(1.ToRegex(), 2.ToRegex(), 3.ToRegex());
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStarred()
        {
            var expected = new StarRegex<int>(123.ToRegex());
            var actual = 123.ToRegex().Starred();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOptional()
        {
            var expected = new SumRegex<int>(new EpsilonRegex<int>(), 123.ToRegex());
            var actual = 123.ToRegex().Optional();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCreateList()
        {
            var expected = new SumRegex<int>(
                new EpsilonRegex<int>(),
                RegexUtils.Concat(123.ToRegex(), RegexUtils.Concat(0.ToRegex(), 123.ToRegex()).Starred()));
            var actual = RegexUtils.CreateListRegex(123.ToRegex(), 0.ToRegex());
            Assert.AreEqual(expected, actual);
        }
    }
}