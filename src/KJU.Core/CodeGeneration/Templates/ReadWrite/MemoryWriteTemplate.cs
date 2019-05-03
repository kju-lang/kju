namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    internal class MemoryWriteTemplate : InstructionTemplate
    {
        public MemoryWriteTemplate()
            : base(new MemoryWrite(null, null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var to = fill.GetRegister(0);
            var from = fill.GetRegister(1);
            return new MovRegisterMemoryInstruction(to, from);
        }

        private class MovRegisterMemoryInstruction : Instruction
        {
            private readonly VirtualRegister to;
            private readonly VirtualRegister from;

            public MovRegisterMemoryInstruction(
                VirtualRegister to,
                VirtualRegister from)
                : base(
                    new List<VirtualRegister> { from },
                    new List<VirtualRegister> { to },
                    new List<Tuple<VirtualRegister, VirtualRegister>>())
            {
                this.to = to;
                this.from = from;
            }

            public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var toHardware = this.to.ToHardware(registerAssignment);
                var fromHardware = this.from.ToHardware(registerAssignment);
                yield return $"mov [{toHardware}], {fromHardware}";
            }
        }
    }
}