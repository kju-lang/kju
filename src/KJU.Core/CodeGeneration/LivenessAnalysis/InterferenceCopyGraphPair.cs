namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System.Collections.Generic;
    using Intermediate;

    public class InterferenceCopyGraphPair
    {
        public InterferenceCopyGraphPair(
            IReadOnlyDictionary<VirtualRegister, IReadOnlyList<VirtualRegister>> interferenceGraph,
            IReadOnlyDictionary<VirtualRegister, IReadOnlyList<VirtualRegister>> copyGraph)
        {
            this.InterferenceGraph = interferenceGraph;
            this.CopyGraph = copyGraph;
        }

        public IReadOnlyDictionary<VirtualRegister, IReadOnlyList<VirtualRegister>> InterferenceGraph { get; }

        public IReadOnlyDictionary<VirtualRegister, IReadOnlyList<VirtualRegister>> CopyGraph { get; }
    }
}