namespace KJU.Core.CodeGeneration.RegisterAllocation
{
    using System.Collections.Generic;
    using Intermediate;

    public class RegisterAllocationResult
    {
        public RegisterAllocationResult(
            IReadOnlyDictionary<VirtualRegister, HardwareRegister> allocation,
            IReadOnlyCollection<VirtualRegister> spilled)
        {
            this.Allocation = allocation;
            this.Spilled = spilled;
        }

        public IReadOnlyDictionary<VirtualRegister, HardwareRegister> Allocation { get; }

        public IReadOnlyCollection<VirtualRegister> Spilled { get; }
    }
}
