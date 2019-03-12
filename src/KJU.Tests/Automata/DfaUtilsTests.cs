namespace KJU.Tests.Automata
{
    using KJU.Core.Automata;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DfaUtilsTests
    {
        [TestMethod]
        public void TestGetAllStates()
        {
            var dfa = new ConcreteDfa<bool>();
            Assert.AreEqual(DfaUtils.GetAllStates(dfa).Count, 1);

            dfa.AddEdge(0, 'a', 1);
            dfa.AddEdge(1, 'b', 2);

            Assert.AreEqual(DfaUtils.GetAllStates(dfa).Count, 3);

            dfa.AddEdge(0, 'c', 3);
            dfa.AddEdge(3, 'd', 2);
            dfa.AddEdge(0, 'e', 2);

            Assert.AreEqual(DfaUtils.GetAllStates(dfa).Count, 4);

            dfa.AddEdge(2, 'f', 4);
            dfa.AddEdge(4, 'g', 0);
            dfa.AddEdge(0, 'h', 5);

            Assert.AreEqual(DfaUtils.GetAllStates(dfa).Count, 6);
        }
    }
}