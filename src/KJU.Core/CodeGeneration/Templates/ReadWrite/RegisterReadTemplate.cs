namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    internal class RegisterReadTemplate : InstructionTemplate
    {
        public RegisterReadTemplate()
            : base(new RegisterRead(null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var readFrom = fill.GetRegister(0);
            return new MovRegisterRegisterInstruction(result, readFrom);
        }
    }
}