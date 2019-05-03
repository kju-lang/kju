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
            var labels = Label.WithLabel(sL =>
            {
                var outerEL = Label.WithLabel(eL =>
                {
                    var endTree = new Tree(null, new UnconditionalJump(sL));
                    return (endTree, eL);
                });
                var startTree = new Tree(null, new UnconditionalJump(outerEL));
                return (startTree, new List<Label> { sL, outerEL });
            });

            var startLabel = labels[0];
            var endLabel = labels[1];
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
            var labels = Label.WithLabel(sL =>
            {
                var outerEL = Label.WithLabel(eL =>
                {
                    var endTree = new Tree(null, new FunctionCall(null, sL));
                    return (endTree, eL);
                });
                var startTree = new Tree(null, new FunctionCall(null, outerEL));
                return (startTree, new List<Label> { sL, outerEL });
            });

            var startLabel = labels[0];
            var endLabel = labels[1];

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
            var labels = Label.WithLabel(l0 =>
            {
                var labels12 = Label.WithLabel(l1 =>
                {
                    var label2 = Label.WithLabel(l2 =>
                    {
                        var tree2 = new Tree(null, new ConditionalJump(l1, l2));
                        return (tree2, l2);
                    });
                    var tree1 = new Tree(null, new ConditionalJump(label2, l0));
                    return (tree1, new List<Label> { l1, label2 });
                });
                var tree0 = new Tree(null, new ConditionalJump(labels12[1], labels12[0]));
                return (tree0, new List<Label> { l0, labels12[0], labels12[1] });
            });

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
            var t3 = new Tree(new IntegerImmediateValue(0), new Ret());
            var l3a = new Label(t3);
            var l3b = new Label(t3);

            var l2 = new Label(new Tree(null, new UnconditionalJump(l3b)));
            var l1 = new Label(new Tree(null, new ConditionalJump(l2, l3a)));

            var output = this.linearizer.Linearize(l1);
            var intTreeCount = output.Item1.Count(x => x.Root is IntegerImmediateValue);
            Assert.AreEqual(1, intTreeCount);
        }
    }
}