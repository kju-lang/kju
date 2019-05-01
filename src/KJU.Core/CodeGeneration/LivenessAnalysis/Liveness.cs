namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System.Collections.Generic;
    using Intermediate;

    internal class Liveness
    {
        public Liveness(
            HashSet<VirtualRegister> inLiveness, HashSet<VirtualRegister> outLiveness)
        {
            this.InLiveness = inLiveness;
            this.OutLiveness = outLiveness;
        }

        public HashSet<VirtualRegister> InLiveness { get; }

        public HashSet<VirtualRegister> OutLiveness { get; }
    }
}