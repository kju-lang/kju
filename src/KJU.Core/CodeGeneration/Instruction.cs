namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public abstract class Instruction
    {
        protected Instruction(
            IReadOnlyCollection<VirtualRegister> uses = null,
            IReadOnlyCollection<VirtualRegister> defines = null,
            IReadOnlyCollection<Tuple<VirtualRegister, VirtualRegister>> copies = null)
        {
            this.Uses = uses ?? new List<VirtualRegister>();
            this.Defines = defines ?? new List<VirtualRegister>();
            this.Copies = copies ?? new List<Tuple<VirtualRegister, VirtualRegister>>();
        }

        public IReadOnlyCollection<VirtualRegister> Uses { get; set; }

        public IReadOnlyCollection<VirtualRegister> Defines { get; set; }

        public IReadOnlyCollection<Tuple<VirtualRegister, VirtualRegister>> Copies { get; }

        public abstract IEnumerable<string> ToASM(
            IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment);

        public override string ToString()
        {
            return
                $"{this.GetType().Name}:{{Uses: {string.Join(", ", this.Uses)}, Defines:{string.Join(", ", this.Defines)}}}";
        }
    }
}