namespace KJU.Core.CodeGeneration.LivenessAnalysis
{
    using System.Collections.Generic;
    using FunctionToAsmGeneration;

    public interface ILivenessAnalyzer
    {
        InterferenceCopyGraphPair GetInterferenceCopyGraphs(
            IReadOnlyList<CodeBlock> instructions);
    }
}