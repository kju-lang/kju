namespace KJU.Core.CodeGeneration.CFGLinearizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                {
                    continue;
                }

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
                        ConditionalJump newConditionalJump;
                        if (order.ContainsKey(conditionalJump.FalseTarget) &&
                            !order.ContainsKey(conditionalJump.TrueTarget))
                        {
                            FlipConditionalJumpTargets(current.Tree);
                            newConditionalJump = conditionalJump;
                        }
                        else
                        {
                            newConditionalJump = conditionalJump;
                        }

                        labelsToProcess.Push(newConditionalJump.TrueTarget);
                        labelsToProcess.Push(newConditionalJump.FalseTarget);
                        break;
                    case FunctionCall functionCall:
                        labelsToProcess.Push(functionCall.TargetAfter);
                        break;
                    case Ret _:
                        break;
                    default:
                        throw new NotSupportedException($"Unknown control flow instruction: {current.Tree.ControlFlow}");
                }

                order[current] = processedTrees[current.Tree] = treeTable.Count;
                treeTable.Add(current.Tree);
            }

            var resultTreeTable = EraseUnnecessaryJumps(treeTable, order);
            return new Tuple<IReadOnlyList<Tree>, IReadOnlyDictionary<Label, int>>(resultTreeTable, order);
        }

        private static void FlipConditionalJumpTargets(Tree tree)
        {
            if (tree.ControlFlow is ConditionalJump conditionalJump)
            {
                tree.Root = new UnaryOperation(tree.Root, AST.UnaryOperationType.Not);
                tree.ControlFlow = new ConditionalJump(conditionalJump.FalseTarget, conditionalJump.TrueTarget);
            }
            else
            {
                throw new Exception(
                    $"Expected control flow to be type ConditionalJump. Got: {tree.ControlFlow.GetType()}");
            }
        }

        private static IReadOnlyList<Tree> EraseUnnecessaryJumps(
            IEnumerable<Tree> treeTable, IReadOnlyDictionary<Label, int> order)
        {
            return treeTable.Select((tree, index) =>
            {
                switch (tree.ControlFlow)
                {
                    case UnconditionalJump unconditionalJump:
                        if (order[unconditionalJump.Target] == index + 1)
                        {
                            tree.ControlFlow = new UnconditionalJump(null);
                        }

                        break;
                    case ConditionalJump conditionalJump:
                        if (order[conditionalJump.FalseTarget] == index + 1)
                        {
                            tree.ControlFlow =
                                new ConditionalJump(conditionalJump.TrueTarget, null);
                        }

                        break;
                    case FunctionCall functionCall: // erase TargetAfter???
                        if (order[functionCall.TargetAfter] == index + 1)
                        {
                            tree.ControlFlow = new FunctionCall(functionCall.Func, null);
                        }

                        break;
                }

                return tree;
            }).ToList();
        }
    }
}