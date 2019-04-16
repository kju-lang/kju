namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class CallInstruction : Instruction
    {
        public Function Func { get; set; }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return $"call {this.Func.MangledName}\n";
        }
    }
}
