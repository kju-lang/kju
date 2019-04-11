namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public abstract class Instruction
    {
        protected Instruction(
            IReadOnlyCollection<VirtualRegister> use = null,
            IReadOnlyCollection<VirtualRegister> define = null,
            IReadOnlyCollection<Tuple<VirtualRegister, VirtualRegister>> copy = null)
        {
            this.Use = use ?? new List<VirtualRegister>();
            this.Define = define ?? new List<VirtualRegister>();
            this.Copy = copy ?? new List<Tuple<VirtualRegister, VirtualRegister>>();
        }

        public IReadOnlyCollection<VirtualRegister> Use { get; }

        public IReadOnlyCollection<VirtualRegister> Define { get; }

        public IReadOnlyCollection<Tuple<VirtualRegister, VirtualRegister>> Copy { get; }

        public abstract string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment);
    }
}