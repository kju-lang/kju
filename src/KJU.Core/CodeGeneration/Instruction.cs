namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public abstract class Instruction
    {
        public IReadOnlyCollection<VirtualRegister> Use { get; set; }

        public IReadOnlyCollection<VirtualRegister> Define { get; set; }

        public IReadOnlyCollection<Tuple<VirtualRegister, VirtualRegister>> Copy { get; set; }

        public abstract string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment);
    }
}
