namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OptionalTests
    {
        [TestMethod]
        public void TestSimple()
        {
            Assert.AreEqual(Optional<string>.None(), Optional<string>.None());
            Assert.AreEqual(Optional<string>.Some("foo"), Optional<string>.Some("foo"));
            Assert.AreNotEqual(Optional<string>.Some("foo1"), Optional<string>.Some("foo"));
            Assert.AreNotEqual(Optional<string>.Some("foo1"), Optional<string>.None());
            Assert.AreNotEqual(Optional<string>.Some("foo1"), Optional<object>.Some("foo1"));
            Assert.AreNotEqual(Optional<string>.Some("foo1"), Optional<object>.Some("foo"));
            Assert.AreEqual(Optional<string>.Some("foo").GetHashCode(), Optional<string>.Some("foo").GetHashCode());
            Assert.AreEqual(Optional<string>.None().GetHashCode(), Optional<string>.None().GetHashCode());
        }
    }
}