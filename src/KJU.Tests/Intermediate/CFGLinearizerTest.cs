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
        private readonly CfgLinearizer linearizer = new CfgLinearizer();

        [TestMethod]
        public void UnconditionalJumpTest()
        {
            var startLabel = new Label(null);
            var endLabel = new Label(null);
            var start = new Tree(null, new UnconditionalJump(endLabel));
            var end = new Tree(null, new UnconditionalJump(startLabel));
            startLabel.Tree = start;
            endLabel.Tree = end;
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
            var startLabel = new Label(null);
            var endLabel = new Label(null);
            var start = new Tree(null, new FunctionCall(null, endLabel));
            var end = new Tree(null, new FunctionCall(null, startLabel));
            startLabel.Tree = start;
            endLabel.Tree = end;

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
            var labels = Enumerable.Range(0, 3).Select(_ => new Label(null)).ToList();

            var trees = new List<Tree>
            {
                new Tree(null, new ConditionalJump(labels[2], labels[1])),
                new Tree(null, new ConditionalJump(labels[2], labels[0])),
                new Tree(null, new ConditionalJump(labels[1], labels[2])),
            };

            labels
                .Zip(trees, (label, tree) => new { Label = label, Tree = tree })
                .ToList()
                .ForEach(x => x.Label.Tree = x.Tree);

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
            var l1 = new Label(null);
            var l2 = new Label(null);
            var l3a = new Label(null);
            var l3b = new Label(null);

            var t1 = new Tree(null, new ConditionalJump(l2, l3a));
            var t2 = new Tree(null, new UnconditionalJump(l3b));
            var t3 = new Tree(new IntegerImmediateValue(0), new Ret());

            l1.Tree = t1;
            l2.Tree = t2;
            l3a.Tree = t3;
            l3b.Tree = t3;

            var output = this.linearizer.Linearize(l1);
            var intTreeCount = output.Item1.Count(x => x.Root is IntegerImmediateValue);
            Assert.AreEqual(1, intTreeCount);
        }
    }
}