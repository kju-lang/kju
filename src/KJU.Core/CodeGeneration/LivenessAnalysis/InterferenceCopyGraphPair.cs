namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System.Collections.Generic;
    using Intermediate;

    public class InterferenceCopyGraphPair
    {
        public InterferenceCopyGraphPair(
            IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> interferenceGraph,
            IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> copyGraph)
        {
            this.InterferenceGraph = interferenceGraph;
            this.CopyGraph = copyGraph;
        }

        public IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> InterferenceGraph { get; }

        public IReadOnlyDictionary<VirtualRegister, IReadOnlyCollection<VirtualRegister>> CopyGraph { get; }
    }
}