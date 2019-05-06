namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    public class UsesDefinesTemplate : InstructionTemplate
    {
        public UsesDefinesTemplate()
            : base(new UsesDefinesNode(null, null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var uses = fill.GetCollection(0);
            var defines = fill.GetCollection(1);
            return new UsesDefinesInstruction(uses, defines);
        }

        internal class UsesDefinesInstruction : Instruction
        {
            public UsesDefinesInstruction(
                IReadOnlyCollection<VirtualRegister> uses,
                IReadOnlyCollection<VirtualRegister> defines)
                : base(uses, defines)
            {
            }

            public override IEnumerable<string> ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                yield break;
            }
        }
    }
}