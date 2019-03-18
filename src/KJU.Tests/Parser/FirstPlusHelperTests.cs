namespace KJU.Tests.Parser
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Parser;
    using KJU.Core.Util;
    using KJU.Tests.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FirstPlusHelperTests
    {
        [TestMethod]
        public void TestSimple()
        {
            var state0 = GetMockState();
            var state1 = GetMockState();
            var state2 = GetMockState();
            var state3 = GetMockState();

            var first = new Dictionary<string, IReadOnlyCollection<DfaAndState<string>>>
            {
                { "A", new List<DfaAndState<string>> { state0, state1 } },
                { "B", new List<DfaAndState<string>> { state1 } }
            };

            var follow = new Dictionary<string, IReadOnlyCollection<DfaAndState<string>>>
            {
                { "A", new List<DfaAndState<string>> { state2 } },
                { "C", new List<DfaAndState<string>> { state1 } },
                { "D", new List<DfaAndState<string>> { state3 } }
            };

            var nullables = new List<DfaAndState<string>> { state2, state3 };

            var firstPlusExpected = new Dictionary<string, IReadOnlyCollection<DfaAndState<string>>>
            {
                { "A", new List<DfaAndState<string>> { state0, state1, state2 } },
                { "B", new List<DfaAndState<string>> { state1 } },
                { "D", new List<DfaAndState<string>> { state3 } }
            };

            var firstPlus = FirstPlusHelper<string>.GetFirstPlusSymbols(first, follow, nullables);
            Assert.IsTrue(MappingEquivalence.AreEquivalentCollection(firstPlus, firstPlusExpected));
        }

        private static DfaAndState<string> GetMockState()
        {
            IDfa<Optional<Rule<string>>, string> dfa = new ConcreteDfa<Optional<Rule<string>>, string>();
            return new DfaAndState<string> { Dfa = dfa, State = dfa.StartingState() };
        }
    }
}