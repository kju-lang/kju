namespace KJU.Core.CodeGeneration.RegisterAllocation
{
    using LivenessAnalysis;

    public interface IRegisterAllocator
    {
        RegisterAllocationResult Allocate(InterferenceCopyGraphPair query);
    }
}
