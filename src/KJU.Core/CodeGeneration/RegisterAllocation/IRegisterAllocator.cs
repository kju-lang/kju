namespace KJU.Core.CodeGeneration.RegisterAllocation
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;
    using LivenessAnalysis;

    public interface IRegisterAllocator
    {
        RegisterAllocationResult Allocate(
            InterferenceCopyGraphPair query,
            IReadOnlyCollection<HardwareRegister> allowedHardwareRegisters);
    }
}
