#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System;
    using System.Collections.Generic;
    using Intermediate;

    internal class MovRegisterRegisterInstruction : Instruction
    {
        private readonly VirtualRegister to;
        private readonly VirtualRegister from;

        public MovRegisterRegisterInstruction(
            VirtualRegister to,
            VirtualRegister from)
            : base(
                new List<VirtualRegister> { from },
                new List<VirtualRegister> { to },
                new List<Tuple<VirtualRegister, VirtualRegister>>
                {
                    new Tuple<VirtualRegister, VirtualRegister>(to, from)
                })
        {
            this.to = to;
            this.from = from;
        }

        public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            var toHardware = this.to.ToHardware(registerAssignment);
            var fromHardware = this.from.ToHardware(registerAssignment);

            if (toHardware != fromHardware)
            {
                yield return $"mov {toHardware}, {fromHardware}";
            }
        }
    }
}