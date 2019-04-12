namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    internal class MemoryReadTemplate : InstructionTemplate
    {
        public MemoryReadTemplate()
            : base(new MemoryRead(null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var from = fill.GetRegister(0);
            return new MovRegisterMemoryInstruction(result, from);
        }

        private class MovRegisterMemoryInstruction : Instruction
        {
            private readonly VirtualRegister result;
            private readonly VirtualRegister memoryLocation;

            public MovRegisterMemoryInstruction(
                VirtualRegister result,
                VirtualRegister memoryLocation)
                : base(
                    new List<VirtualRegister> { memoryLocation },
                    new List<VirtualRegister> { result },
                    new List<Tuple<VirtualRegister, VirtualRegister>>())
            {
                this.result = result;
                this.memoryLocation = memoryLocation;
            }

            public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var toHardware = this.result.ToHardware(registerAssignment);
                var fromHardware = this.memoryLocation.ToHardware(registerAssignment);
                return $"mov {toHardware} [{fromHardware}]\n";
            }
        }
    }
}