namespace KJU.Tests.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core;
    using KJU.Core.Automata;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ValueStateTests
    {
        [TestMethod]
        public void TestValues()
        {
            Assert.AreEqual(new ValueState<int>(5) as object, new ValueState<int>(5) as object);
            Assert.AreEqual(new ValueState<int>(5) as IState, new ValueState<int>(5) as IState);
            Assert.AreEqual(new ValueState<int>(5), new ValueState<int>(5));
            Assert.IsTrue(new ValueState<int>(5).Equals(new ValueState<int>(5)));
            Assert.IsTrue((new ValueState<int>(5) as IState).Equals(new ValueState<int>(5)));
            Assert.AreNotEqual(new ValueState<int>(5), new ValueState<int>(6));
            Assert.AreNotEqual(new ValueState<object>(null), new ValueState<string>(null));
            Assert.AreNotEqual(new ValueState<int>(5), new ValueState<string>("foo"));
        }
    }
}