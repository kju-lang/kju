namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public interface ILivenessAnalyzer
    {
        InterferenceCopyGraphPair GetInterferenceCopyGraphs(
            IReadOnlyList<Tuple<Label, IReadOnlyList<Instruction>>> instructions);
    }
}