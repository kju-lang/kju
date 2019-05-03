namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class ClearDFTemplate : InstructionTemplate
    {
        public ClearDFTemplate()
            : base(new ClearDF(), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            return new ClearDFInstruction();
        }

        private class ClearDFInstruction : Instruction
        {
            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                yield return "cld";
            }
        }
    }
}
