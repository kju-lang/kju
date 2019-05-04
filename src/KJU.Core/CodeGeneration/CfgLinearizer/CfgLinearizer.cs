namespace KJU.Core.CodeGeneration.CfgLinearizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;

    public class CfgLinearizer : ICfgLinearizer
    {
        public Tuple<IReadOnlyList<Tree>, IReadOnlyDictionary<ILabel, int>> Linearize(ILabel cfg)
        {
            var order = new Dictionary<ILabel, int>();
            var processedTrees = new Dictionary<Tree, int>();
            var labelsToProcess = new Stack<ILabel>();
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
                            current.Tree = FlipConditionalJumpTargets(current.Tree.Root, conditionalJump);
                            newConditionalJump = (ConditionalJump)current.Tree.ControlFlow;
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
                        throw new NotSupportedException(
                            $"Unknown control flow instruction: {current.Tree.ControlFlow}");
                }

                order[current] = processedTrees[current.Tree] = treeTable.Count;
                treeTable.Add(current.Tree);
            }

            var resultTreeTable = EraseUnnecessaryJumps(treeTable, order);
            return new Tuple<IReadOnlyList<Tree>, IReadOnlyDictionary<ILabel, int>>(resultTreeTable, order);
        }

        private static Tree FlipConditionalJumpTargets(Node operation, ConditionalJump conditionalJump)
        {
            var newOperation = new UnaryOperation(operation, AST.UnaryOperationType.Not);
            var newControlFlow = new ConditionalJump(conditionalJump.FalseTarget, conditionalJump.TrueTarget);
            return new Tree(newOperation, newControlFlow);
        }

        private static IReadOnlyList<Tree> EraseUnnecessaryJumps(
            IEnumerable<Tree> treeTable, IReadOnlyDictionary<ILabel, int> order)
        {
            return treeTable.Select((tree, index) =>
            {
                ControlFlowInstruction controlFlow;
                switch (tree.ControlFlow)
                {
                    case UnconditionalJump unconditionalJump:
                        controlFlow = order[unconditionalJump.Target] == index + 1
                            ? new UnconditionalJump(null)
                            : tree.ControlFlow;

                        break;
                    case ConditionalJump conditionalJump:
                        controlFlow = order[conditionalJump.FalseTarget] == index + 1
                            ? new ConditionalJump(conditionalJump.TrueTarget, null)
                            : tree.ControlFlow;

                        break;
                    case FunctionCall functionCall: // erase TargetAfter???
                        controlFlow = order[functionCall.TargetAfter] == index + 1
                            ? new FunctionCall(functionCall.Func, null)
                            : tree.ControlFlow;

                        break;
                    default:
                        controlFlow = tree.ControlFlow;
                        break;
                }
                var treeRoot = tree.Root;
                return new Tree(treeRoot, controlFlow);
            }).ToList();
        }
    }
}