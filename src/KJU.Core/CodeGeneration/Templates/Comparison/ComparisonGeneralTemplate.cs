#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Comparison
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AST;
    using Intermediate;

    public class ComparisonGeneralTemplate : InstructionTemplate
    {
        private readonly ComparisonType operationType;

        public ComparisonGeneralTemplate(ComparisonType operationType)
            : base(new Intermediate.Comparison(null, null, operationType), 1)
        {
            this.operationType = operationType;
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new ComparisonGeneralInstruction(lhs, rhs, result, this.operationType);
        }
    }
}