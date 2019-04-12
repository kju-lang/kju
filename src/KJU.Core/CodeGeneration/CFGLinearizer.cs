namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public class CFGLinearizer
    {
        public Tuple<IReadOnlyList<Tree>, IReadOnlyDictionary<Label, int>> Linearize(Label cfg)
        {
            var order = new Dictionary<Label, int>();
            var processedTrees = new Dictionary<Tree, int>();
            var labelsToProcess = new Stack<Label>();
            var treeTable = new List<Tree>();
            labelsToProcess.Push(cfg);

            while (labelsToProcess.Count > 0)
            {
                var current = labelsToProcess.Pop();
                if (order.ContainsKey(current))
                    continue;
                if (processedTrees.ContainsKey(current.Tree))
                {
                    order[current] = processedTrees[current.Tree];
                    continue;
                }

                switch (current.Tree.ControlFlow)
                {
                    case UnconditionalJump unconditionalJump:
                        labelsToProcess.Push(unconditionalJump.Target);
                        break;
                    case ConditionalJump conditionalJump:
                        if (order.ContainsKey(conditionalJump.FalseTarget) && !order.ContainsKey(conditionalJump.TrueTarget))
                        {
                            this.FlipConditionalJumpTargets(current.Tree);
                            conditionalJump = (ConditionalJump)current.Tree.ControlFlow;
                        }

                        labelsToProcess.Push(conditionalJump.TrueTarget);
                        labelsToProcess.Push(conditionalJump.FalseTarget);
                        break;
                    case FunctionCall functionCall:
                        labelsToProcess.Push(functionCall.TargetAfter);
                        break;
                }

                order[current] = processedTrees[current.Tree] = treeTable.Count;
                treeTable.Add(current.Tree);
            }

            this.EraseUnnecessaryJumps(treeTable, order);
            return new Tuple<IReadOnlyList<Tree>, IReadOnlyDictionary<Label, int>>(treeTable, order);
        }

        private void FlipConditionalJumpTargets(Tree tree)
        {
            if (!(tree.ControlFlow is ConditionalJump))
                throw new Exception("function FlipConditionalJumpTargets with tree.ControlFlow of type ConditionalJump");
            var conditionalJump = (ConditionalJump)tree.ControlFlow;
            tree.Root = new UnaryOperation(tree.Root, AST.UnaryOperationType.Not);
            tree.ControlFlow = new ConditionalJump(conditionalJump.FalseTarget, conditionalJump.TrueTarget);
        }

        private void EraseUnnecessaryJumps(IReadOnlyList<Tree> treeTable, IReadOnlyDictionary<Label, int> order)
        {
            var n = treeTable.Count;
            for (var i = 0; i < n - 1; i++)
            {
                switch (treeTable[i].ControlFlow)
                {
                    case UnconditionalJump unconditionalJump:
                        if (order[unconditionalJump.Target] == i + 1)
                            treeTable[i].ControlFlow = new UnconditionalJump(null);
                        break;
                    case ConditionalJump conditionalJump:
                        if (order[conditionalJump.FalseTarget] == i + 1)
                        {
                            treeTable[i].ControlFlow =
                                new ConditionalJump(conditionalJump.TrueTarget, null);
                        }

                        break;
                    case FunctionCall functionCall: // erase TargetAfter???
                        if (order[functionCall.TargetAfter] == i + 1)
                            treeTable[i].ControlFlow = new FunctionCall(functionCall.Func, null);
                        break;
                }
            }
        }
    }
}
