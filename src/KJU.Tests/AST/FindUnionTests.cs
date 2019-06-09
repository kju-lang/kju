namespace KJU.Tests.AST
{
    using System;
    using KJU.Core.AST.TypeChecker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FindUnionTests
    {
        [TestMethod]
        public void TestInit()
        {
            var fu = new FindUnion<int>(FixedArbiter<int>);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, fu.GetParent(i));
        }

        [TestMethod]
        public void TestSimpleUnion()
        {
            var fu = new FindUnion<int>(FixedArbiter<int>);
            for (int i = 2; i < 10; i++)
                fu.Union(i % 2, i);
            // since 1st argument always wins, the representants should be guaranteed to be 0 and 1
            for (int i = 0; i < 10; i += 2) {
                Assert.AreEqual(0, fu.GetRepresentant(i));
                Assert.AreEqual(1, fu.GetRepresentant(i + 1));
            }
        }

        [TestMethod]
        public void TestSimpleCheckpoint()
        {
            var fu = new FindUnion<int>(FixedArbiter<int>);
            fu.PushCheckpoint();
            for (int i = 2; i < 10; i++)
                fu.Union(i % 2, i);
            for (int i = 0; i < 10; i += 2) {
                Assert.AreEqual(0, fu.GetRepresentant(i));
                Assert.AreEqual(1, fu.GetRepresentant(i + 1));
            }

            fu.PopCheckpoint();
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, fu.GetRepresentant(i));
        }

        [TestMethod]
        public void TestCheckpointDoubleRestore()
        {
            var fu = new FindUnion<int>(FixedArbiter<int>);

            fu.PushCheckpoint();
            for (int i = 2; i < 10; i++)
                fu.Union(i % 2, i);
            for (int i = 0; i < 10; i += 2) {
                Assert.AreEqual(0, fu.GetRepresentant(i));
                Assert.AreEqual(1, fu.GetRepresentant(i + 1));
            }

            fu.PushCheckpoint();
            fu.Union(0, 1);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(0, fu.GetRepresentant(i));

            fu.PopCheckpoint();
            for (int i = 0; i < 10; i += 2) {
                Assert.AreEqual(0, fu.GetRepresentant(i));
                Assert.AreEqual(1, fu.GetRepresentant(i + 1));
            }

            fu.PopCheckpoint();
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, fu.GetRepresentant(i));
        }

        [TestMethod]
        public void TestSmartArbiter()
        {
            var fu = new FindUnion<int>(TakeEvenArbiter);
            fu.PushCheckpoint();
            fu.PushCheckpoint();
            fu.PushCheckpoint();
            fu.Union(0, 1);
            Assert.AreEqual(0, fu.GetRepresentant(0));
            Assert.AreEqual(0, fu.GetRepresentant(1));

            fu.PopCheckpoint();
            fu.Union(1, 0);
            Assert.AreEqual(0, fu.GetRepresentant(0));
            Assert.AreEqual(0, fu.GetRepresentant(1));

            fu.PopCheckpoint();
            for (int i = 3; i < 10; i += 2)
                fu.Union(1, i);
            fu.Union(0, 1);
            Assert.AreEqual(0, fu.GetRepresentant(0));
            for (int i = 1; i < 10; i += 2)
                Assert.AreEqual(0, fu.GetRepresentant(i));

            fu.PopCheckpoint();
            Assert.AreEqual(0, fu.GetRepresentant(0));
            for (int i = 1; i < 10; i += 2)
                Assert.AreEqual(i, fu.GetRepresentant(i));
        }

        private static int FixedArbiter<T>(T a, T b)
        {
            return 1;
        }

        private static int TakeEvenArbiter(int a, int b)
        {
            if (a % 2 == 0 && b % 2 == 1)
                return 1;
            if (b % 2 == 0 && a % 2 == 1)
                return -1;
            return 0;
        }
    }
}