namespace KJU.Core.CodeGeneration.Templates.Comments
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class CommentInstruction : Instruction
    {
        private readonly string value;

        public CommentInstruction(string value)
        {
            this.value = value;
        }

        public override IEnumerable<string> ToASM(
            IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            yield return $"; {this.value}";
        }
    }
}