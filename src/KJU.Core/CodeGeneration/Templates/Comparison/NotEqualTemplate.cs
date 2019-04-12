namespace KJU.Core.CodeGeneration.Templates.Comparison
{
    using System.Collections.Generic;
    using AST;
    using Intermediate;

    public class NotEqualTemplate : InstructionTemplate
    {
        public NotEqualTemplate()
            : base(new Intermediate.Comparison(null, null, ComparisonType.NotEqual), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new ComparisonInstruction(lhs, rhs, result, "jne");
        }
    }
}