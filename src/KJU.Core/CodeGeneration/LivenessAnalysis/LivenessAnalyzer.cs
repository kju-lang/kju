#pragma warning disable SA1008  // Opening parenthesis must not be preceded by a space.

namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FunctionToAsmGeneration;
    using Intermediate;
    using Util;
    using ControlFlowGraph =
        System.Collections.Generic.IReadOnlyDictionary<Instruction,
            System.Collections.Generic.IReadOnlyCollection<Instruction>>;

    public class LivenessAnalyzer : ILivenessAnalyzer
    {
        public InterferenceCopyGraphPair GetInterferenceCopyGraphs(
            IReadOnlyList<CodeBlock> instructions)
        {
            var cfg = GetInstructionCFG(instructions);
            var reverseCFG = GraphReverser.ReverseGraph(cfg);
            var liveness = GetLivenessSets(reverseCFG);
            var interferenceGraph = CreateInterferenceGraph(reverseCFG, liveness);
            var copyGraph = CreateCopyGraph(reverseCFG);
            return new InterferenceCopyGraphPair(interferenceGraph, copyGraph);
        }

        private static ControlFlowGraph GetInstructionCFG(IReadOnlyList<CodeBlock> codeBlocks)
        {
            var labelToFirstInstruction = codeBlocks
                .ToDictionary(codeBlock => codeBlock.Label, codeBlock => codeBlock.Instructions[0]);

            var cfg = codeBlocks
                .SelectMany(codeBlock => codeBlock.Instructions)
                .Distinct()
                .ToDictionary(instruction => instruction, _ => new HashSet<Instruction>());

            for (int i = 0; i < codeBlocks.Count; i++)
            {
                var codeBlock = codeBlocks[i];
                var label = codeBlock.Label;
                var instructions = codeBlock.Instructions;
                instructions
                    .Zip(instructions.Skip(1), (current, next) => new { Current = current, Next = next })
                    .ToList()
                    .ForEach(x => cfg[x.Current].Add(x.Next));

                bool connectedWithNext;
                var lastInstruction = instructions[instructions.Count - 1];
                switch (label.Tree.ControlFlow)
                {
                    case UnconditionalJump unconditionalJump:
                        connectedWithNext = unconditionalJump.Target == null;
                        if (!connectedWithNext)
                        {
                            cfg[lastInstruction].Add(labelToFirstInstruction[unconditionalJump.Target]);
                        }

                        break;
                    case ConditionalJump conditionalJump:
                        cfg[lastInstruction].Add(labelToFirstInstruction[conditionalJump.TrueTarget]);
                        connectedWithNext = conditionalJump.FalseTarget == null;
                        if (!connectedWithNext)
                        {
                            cfg[lastInstruction].Add(labelToFirstInstruction[conditionalJump.FalseTarget]);
                        }

                        break;
                    case FunctionCall functionCall:
                        connectedWithNext = functionCall.TargetAfter == null;
                        if (!connectedWithNext)
                        {
                            cfg[lastInstruction].Add(labelToFirstInstruction[functionCall.TargetAfter]);
                        }

                        break;
                    case Ret _:
                        connectedWithNext = false;
                        break;
                    default:
                        throw new Exception("Unexpected ControlFlow type");
                }

                if (connectedWithNext && i + 1 < codeBlocks.Count)
                {
                    cfg[lastInstruction].Add(labelToFirstInstruction[codeBlocks[i + 1].Label]);
                }
            }

            return cfg.ToDictionary(
                elem => elem.Key,
                elem => (IReadOnlyCollection<Instruction>)elem.Value);
        }

        private static IReadOnlyDictionary<Instruction, Liveness> GetLivenessSets(
            ControlFlowGraph reverseCFG)
        {
            var liveness = reverseCFG.Keys.ToDictionary(instr => instr, instr =>
            {
                var inLiveness = new HashSet<VirtualRegister>(instr.Uses);
                var outLiveness = new HashSet<VirtualRegister>(instr.Defines);
                return new Liveness(inLiveness, outLiveness);
            });

            var initialInstructions = reverseCFG
                .SelectMany(
                    kvp => kvp.Value
                        .ToList()
                        .Select(preInstr => new InstructionLiveness(preInstr, liveness[kvp.Key].InLiveness)));

            var instructionsToProcess = new Queue<InstructionLiveness>(initialInstructions);

            while (instructionsToProcess.Count > 0)
            {
                var currentLiveness = instructionsToProcess.Dequeue();
                var instruction = currentLiveness.Instruction;
                var vrToCheck = currentLiveness.Liveness;
                var newVrs = vrToCheck.Where(vr => !liveness[instruction].OutLiveness.Contains(vr)).ToList();
                if (newVrs.Count == 0)
                {
                    continue;
                }

                liveness[instruction].OutLiveness.UnionWith(newVrs);
                liveness[instruction].InLiveness.UnionWith(newVrs);
                reverseCFG[instruction].ToList().ForEach(preInstr =>
                    instructionsToProcess.Enqueue(
                        new InstructionLiveness(preInstr, new HashSet<VirtualRegister>(newVrs))));
            }

            return liveness;
        }

        private static IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>>
            CreateInterferenceGraph(
                ControlFlowGraph reverseCFG,
                IReadOnlyDictionary<Instruction, Liveness> liveness)
        {
            var interferenceGraph = GetEmptyGraph(reverseCFG);

            foreach (var instruction in reverseCFG.Keys)
            {
                foreach (var vr in instruction.Defines)
                {
                    foreach (var outLiveVr in liveness[instruction].OutLiveness)
                    {
                        if (vr != outLiveVr)
                        {
                            interferenceGraph[vr].Add(outLiveVr);
                            interferenceGraph[outLiveVr].Add(vr);
                        }
                    }
                }
            }

            return interferenceGraph
                .ToDictionary(elem => elem.Key, elem => (IReadOnlyCollection<VirtualRegister>)elem.Value);
        }

        private static IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> CreateCopyGraph(
            ControlFlowGraph reverseCFG)
        {
            var copyGraph = GetEmptyGraph(reverseCFG);

            reverseCFG.Keys.ToList().ForEach(instr =>
            {
                instr.Copies.Where(x => x.Item1 != x.Item2).ToList().ForEach(pair =>
                {
                    var (item1, item2) = pair;
                    copyGraph[item1].Add(item2);
                    copyGraph[item2].Add(item1);
                });
            });
            return copyGraph
                .ToDictionary(
                    elem => elem.Key,
                    elem => (IReadOnlyCollection<VirtualRegister>)elem.Value);
        }

        private static Dictionary<VirtualRegister, HashSet<VirtualRegister>> GetEmptyGraph(
            ControlFlowGraph reverseCFG)
        {
            return reverseCFG.Keys
                .SelectMany(instr => instr.Uses.Concat(instr.Defines))
                .Distinct()
                .ToDictionary(vr => vr, _ => new HashSet<VirtualRegister>());
        }
    }
}