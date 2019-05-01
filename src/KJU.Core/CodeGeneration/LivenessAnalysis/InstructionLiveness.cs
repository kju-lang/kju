namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System.Collections.Generic;
    using Intermediate;

    internal class InstructionLiveness
    {
        public InstructionLiveness(Instruction instruction, HashSet<VirtualRegister> liveness)
        {
            this.Instruction = instruction;
            this.Liveness = liveness;
        }

        public Instruction Instruction { get; }

        public HashSet<VirtualRegister> Liveness { get; }
    }
}