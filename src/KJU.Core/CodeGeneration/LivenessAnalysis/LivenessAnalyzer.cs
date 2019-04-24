#pragma warning disable SA1008  // Opening parenthesis must not be preceded by a space.

namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Intermediate;
    using KJU.Core.Util;

    public class LivenessAnalyzer : ILivenessAnalyzer
    {
        public InterferenceCopyGraphPair GetInterferenceCopyGraphs(IReadOnlyList<Tuple<Label, IReadOnlyList<Instruction>>> instructions)
        {
            var cfg = this.GetInstructionCFG(instructions);
            var reverseCFG = GraphReverser.ReverseGraph(cfg);
            var liveness = this.GetLivenessSets(reverseCFG);
            return new InterferenceCopyGraphPair(
                this.CreateInterferenceGraph(reverseCFG, liveness),
                this.CreateCopyGraph(reverseCFG));
        }

        public InterferenceCopyGraphPair GetInterferenceCopyGraphs(IReadOnlyCollection<KeyValuePair<Label, IReadOnlyList<Instruction>>> instructions)
        {
            return this.GetInterferenceCopyGraphs(instructions.Select(x => new Tuple<Label, IReadOnlyList<Instruction>>(x.Key, x.Value)).ToList());
        }

        private IReadOnlyDictionary<Instruction, IReadOnlyCollection<Instruction>> GetInstructionCFG(
            IReadOnlyList<Tuple<Label, IReadOnlyList<Instruction>>> instructions)
        {
            var cfg = new Dictionary<Instruction, HashSet<Instruction>>();
            var labelToFirstInstruction = new Dictionary<Label, Instruction>();
            instructions.ToList().ForEach(basicBlock =>
            {
                var (lab, instructionBlock) = basicBlock;
                instructionBlock.ToList().ForEach(instr => cfg.Add(instr, new HashSet<Instruction>()));
                labelToFirstInstruction.Add(lab, instructionBlock[0]);
            });
            for (int i = 0; i < instructions.Count; i++)
            {
                var (label, instructionBlock) = instructions[i];
                for (int k = 0; k < instructionBlock.Count - 1; k++)
                    cfg[instructionBlock[k]].Add(instructionBlock[k + 1]);

                bool connectedWithNext;
                var lastInstruction = instructionBlock[instructionBlock.Count - 1];
                switch (label.Tree.ControlFlow)
                {
                    case UnconditionalJump unconditionalJump:
                        connectedWithNext = unconditionalJump.Target == null;
                        if (unconditionalJump.Target != null)
                            cfg[lastInstruction].Add(labelToFirstInstruction[unconditionalJump.Target]);
                        break;
                    case ConditionalJump conditionalJump:
                        cfg[lastInstruction].Add(labelToFirstInstruction[conditionalJump.TrueTarget]);
                        connectedWithNext = conditionalJump.FalseTarget == null;
                        if (conditionalJump.FalseTarget != null)
                            cfg[lastInstruction].Add(labelToFirstInstruction[conditionalJump.FalseTarget]);
                        break;
                    case FunctionCall functionCall:
                        connectedWithNext = functionCall.TargetAfter == null;
                        if (functionCall.TargetAfter != null)
                            cfg[lastInstruction].Add(labelToFirstInstruction[functionCall.TargetAfter]);
                        break;
                    case Ret _:
                        connectedWithNext = false;
                        break;
                    default:
                        throw new Exception("unexpected ControlFlow type");
                }

                if (connectedWithNext && i + 1 < instructions.Count)
                    cfg[lastInstruction].Add(labelToFirstInstruction[instructions[i + 1].Item1]);
            }

            return cfg
                .ToDictionary(elem => elem.Key, elem => (IReadOnlyCollection<Instruction>)elem.Value);
        }

        private IReadOnlyDictionary<Instruction, InstructionLiveness> GetLivenessSets(
            IReadOnlyDictionary<Instruction, IReadOnlyCollection<Instruction>> reverseCFG)
        {
            var liveness = new Dictionary<Instruction, InstructionLiveness>();
            var instructionToProcess = new Queue<Tuple<Instruction, IEnumerable<VirtualRegister>>>();
            reverseCFG.Keys.ToList().ForEach(instr =>
            {
                liveness.Add(instr, new InstructionLiveness(instr.Uses, instr.Defines));
                reverseCFG[instr].ToList().ForEach(preInstr => instructionToProcess.
                    Enqueue(new Tuple<Instruction, IEnumerable<VirtualRegister>>(preInstr, liveness[instr].InLiveness)));
            });

            while (instructionToProcess.Count > 0)
            {
                var (instruction, vrToCheck) = instructionToProcess.Dequeue();
                var newVrs = vrToCheck.Where(vr => !liveness[instruction].OutLiveness.Contains(vr)).ToList();
                if (newVrs.Count == 0)
                    continue;

                liveness[instruction].OutLiveness.UnionWith(newVrs);
                liveness[instruction].InLiveness.UnionWith(newVrs);
                reverseCFG[instruction].ToList().ForEach(preInstr => instructionToProcess.
                    Enqueue(new Tuple<Instruction, IEnumerable<VirtualRegister>>(preInstr, newVrs)));
            }

            return liveness;
        }

        private IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> CreateInterferenceGraph(
            IReadOnlyDictionary<Instruction, IReadOnlyCollection<Instruction>> reverseCFG, IReadOnlyDictionary<Instruction, InstructionLiveness> liveness)
        {
            var interferenceGraph = new Dictionary<VirtualRegister, HashSet<VirtualRegister>>();
            reverseCFG.Keys.ToList().ForEach(instr =>
            {
                instr.Uses.ToList().Where(vr => !interferenceGraph.ContainsKey(vr)).Distinct()
                    .ToList().ForEach(vr => interferenceGraph.Add(vr, new HashSet<VirtualRegister>()));
                instr.Defines.ToList().Where(vr => !interferenceGraph.ContainsKey(vr)).Distinct()
                    .ToList().ForEach(vr => interferenceGraph.Add(vr, new HashSet<VirtualRegister>()));
            });

            foreach (var instruction in reverseCFG.Keys)
            {
                foreach (var vr in instruction.Defines)
                {
                    foreach (var outLiveVr in liveness[instruction].OutLiveness)
                    {
                        interferenceGraph[vr].Add(outLiveVr);
                        interferenceGraph[outLiveVr].Add(vr);
                    }
                }
            }

            interferenceGraph.Keys.ToList().ForEach(vr => interferenceGraph[vr].Remove(vr));

            return interferenceGraph
                .ToDictionary(elem => elem.Key, elem => (IReadOnlyCollection<VirtualRegister>)elem.Value);
        }

        private IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> CreateCopyGraph(
            IReadOnlyDictionary<Instruction, IReadOnlyCollection<Instruction>> reverseCFG)
        {
            var copyGraph = new Dictionary<VirtualRegister, HashSet<VirtualRegister>>();
            reverseCFG.Keys.ToList().ForEach(instr =>
            {
                instr.Uses.ToList().Where(vr => !copyGraph.ContainsKey(vr)).Distinct()
                    .ToList().ForEach(vr => copyGraph.Add(vr, new HashSet<VirtualRegister>()));
                instr.Defines.ToList().Where(vr => !copyGraph.ContainsKey(vr)).Distinct()
                    .ToList().ForEach(vr => copyGraph.Add(vr, new HashSet<VirtualRegister>()));
            });
            reverseCFG.Keys.ToList().ForEach(instr =>
            {
                instr.Copies.ToList().ForEach(pair =>
                {
                    var (item1, item2) = pair;
                    copyGraph[item1].Add(item2);
                    copyGraph[item2].Add(item1);
                });
            });
            copyGraph.Keys.ToList().ForEach(vr => copyGraph[vr].Remove(vr));
            return copyGraph
                .ToDictionary(elem => elem.Key, elem => (IReadOnlyCollection<VirtualRegister>)elem.Value);
        }

        private class InstructionLiveness
        {
            public InstructionLiveness()
            {
                this.InLiveness = new HashSet<VirtualRegister>();
                this.OutLiveness = new HashSet<VirtualRegister>();
            }

            public InstructionLiveness(IReadOnlyCollection<VirtualRegister> inLiveness, IReadOnlyCollection<VirtualRegister> outLiveness)
            {
                this.InLiveness = new HashSet<VirtualRegister>(inLiveness);
                this.OutLiveness = new HashSet<VirtualRegister>(outLiveness);
            }

            public HashSet<VirtualRegister> InLiveness { get; }

            public HashSet<VirtualRegister> OutLiveness { get; }
        }
    }
}