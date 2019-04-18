namespace KJU.Core.CodeGeneration.RegisterAllocation
{
    using System.Collections.Generic;
    using Intermediate;
    using LivenessAnalysis;

    public class RegisterAllocator : IRegisterAllocator
    {
        public RegisterAllocationResult Allocate(
            InterferenceCopyGraphPair query,
            IReadOnlyList<HardwareRegister> allowedHardwareRegisters)
        {
            throw new System.NotImplementedException();
        }
    }
}