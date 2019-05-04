namespace KJU.Tests.Intermediate
{
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.CodeGeneration.CfgLinearizer;
    using KJU.Core.Intermediate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CfgLinearizerTest
    {
        private CfgLinearizer linearizer = new CfgLinearizer();

        [TestMethod]
        public void UnconditionalJumpTest()
        {
            var start = new Tree(null);
            var startLabel = new Label(start);
            var end = new Tree(null);
            var endLabel = new Label(end);
            start.ControlFlow = new UnconditionalJump(endLabel);
            end.ControlFlow = new UnconditionalJump(startLabel);

            var output = this.linearizer.Linearize(startLabel);
            var treeTable = output.Item1;
            var order = output.Item2;

            Assert.AreEqual(0, order[startLabel]);
            Assert.AreEqual(1, order[endLabel]);
            Assert.IsNull(((UnconditionalJump)treeTable[0].ControlFlow).Target);
            Assert.AreEqual(startLabel, ((UnconditionalJump)treeTable[1].ControlFlow).Target);
        }

        [TestMethod]
        public void FunctionCallTest()
        {
            var start = new Tree(null);
            var startLabel = new Label(start);
            var end = new Tree(null);
            var endLabel = new Label(end);
            start.ControlFlow = new FunctionCall(null, endLabel);
            end.ControlFlow = new FunctionCall(null, startLabel);

            var output = this.linearizer.Linearize(startLabel);
            var treeTable = output.Item1;
            var order = output.Item2;

            Assert.AreEqual(0, order[startLabel]);
            Assert.AreEqual(1, order[endLabel]);
            Assert.IsNull(((FunctionCall)treeTable[0].ControlFlow).TargetAfter);
            Assert.AreEqual(startLabel, ((FunctionCall)treeTable[1].ControlFlow).TargetAfter);
        }

        [TestMethod]
        public void ConditionalJumpTest()
        {
            var trees = new List<Tree>();
            var labels = new List<Label>();
            for (int i = 0; i < 3; i++)
            {
                trees.Add(new Tree(null));
                labels.Add(new Label(trees[i]));
            }

            trees[0].ControlFlow = new ConditionalJump(labels[2], labels[1]);
            trees[1].ControlFlow = new ConditionalJump(labels[2], labels[0]);
            trees[2].ControlFlow = new ConditionalJump(labels[1], labels[2]);

            var output = this.linearizer.Linearize(labels[0]);
            var treeTable = output.Item1;
            var order = output.Item2;

            Assert.AreEqual(0, order[labels[0]]);
            Assert.AreEqual(1, order[labels[1]]);
            Assert.AreEqual(2, order[labels[2]]);
            var conditionalJump = treeTable[0].ControlFlow as ConditionalJump;
            Assert.IsNull(conditionalJump.FalseTarget);
            Assert.AreEqual(labels[2], conditionalJump.TrueTarget);
            conditionalJump = treeTable[1].ControlFlow as ConditionalJump;
            Assert.IsNull(conditionalJump.FalseTarget);
            Assert.AreEqual(labels[0], conditionalJump.TrueTarget);
            Assert.IsTrue(treeTable[1].Root is UnaryOperation);
            Assert.IsTrue(((UnaryOperation)treeTable[1].Root).Type.Equals(Core.AST.UnaryOperationType.Not));
            conditionalJump = treeTable[2].ControlFlow as ConditionalJump;
            Assert.AreEqual(labels[2], conditionalJump.FalseTarget);
            Assert.AreEqual(labels[1], conditionalJump.TrueTarget);
        }

        [TestMethod]
        public void DoubleLabelTest()
        {
            var t1 = new Tree(null);
            var t2 = new Tree(null);
            var t3 = new Tree(new IntegerImmediateValue(0), new Ret());

            var l1 = new Label(t1);
            var l2 = new Label(t2);
            var l3a = new Label(t3);
            var l3b = new Label(t3);

            t1.ControlFlow = new ConditionalJump(l2, l3a);
            t2.ControlFlow = new UnconditionalJump(l3b);

            var output = this.linearizer.Linearize(l1);
            var intTreeCount = output.Item1.Count(x => x.Root is IntegerImmediateValue);
            Assert.AreEqual(1, intTreeCount);
        }
    }
}