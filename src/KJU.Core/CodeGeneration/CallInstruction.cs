namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class CallInstruction : Instruction
    {
        public CallInstruction(Function function)
            : base(
                new List<VirtualRegister> { HardwareRegister.RSP },
                new List<VirtualRegister> { HardwareRegister.RSP })
        {
            this.Function = function;
        }

        public Function Function { get; }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return $"call {this.Function.MangledName}\n";
        }
    }
}