namespace KJU.Core.CodeGeneration.RegisterAllocation
{
    using System.Collections.Generic;
    using Intermediate;
    using LivenessAnalysis;

    public interface IRegisterAllocator
    {
        RegisterAllocationResult Allocate(
            InterferenceCopyGraphPair query,
            IReadOnlyList<HardwareRegister> allowedHardwareRegisters);
    }
}
