namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System;
    using System.Collections.Generic;
    using Intermediate;
    using KJU.Core.CodeGeneration.FunctionToAsmGeneration;

    public interface ILivenessAnalyzer
    {
        InterferenceCopyGraphPair GetInterferenceCopyGraphs(
            IReadOnlyList<CodeBlock> instructions);
    }
}