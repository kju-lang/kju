namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    internal class RegisterWriteTemplate : InstructionTemplate
    {
        public RegisterWriteTemplate()
            : base(new RegisterWrite(null, null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var to = fill.GetRegister(0);
            var from = fill.GetRegister(1);
            return new MovRegisterRegisterInstruction(to, from);
        }
    }
}