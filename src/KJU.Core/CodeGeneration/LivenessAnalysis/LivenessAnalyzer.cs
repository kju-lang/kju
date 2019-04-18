namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    public class LivenessAnalyzer : ILivenessAnalyzer
    {
        public InterferenceCopyGraphPair GetInterferenceCopyGraphs(IReadOnlyList<Tuple<Label, IReadOnlyList<Instruction>>> instructions)
        {
            throw new NotImplementedException();
        }
    }
}