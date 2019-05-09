namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System;
    using System.Collections.Generic;
    using AST;
    using Intermediate;

    internal class MemoryReadLeaTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return new List<InstructionTemplate>
            {
                new MemoryReadLeaTemplate(new RegisterRead(null), new IntegerImmediateValue()),
                new MemoryReadLeaTemplate(new IntegerImmediateValue(), new RegisterRead(null)),
            };
        }

        private class MemoryReadLeaTemplate : InstructionTemplate
        {
            private readonly Intermediate.Node lhs;
            private readonly Intermediate.Node rhs;

            public MemoryReadLeaTemplate(Intermediate.Node lhs, Intermediate.Node rhs)
                : base(
                    new MemoryRead(
                        new ArithmeticBinaryOperation(
                            ArithmeticOperationType.Addition,
                            lhs,
                            rhs)),
                    1000)
            {
                this.lhs = lhs;
                this.rhs = rhs;
            }

            public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
            {
                VirtualRegister baseRegister;
                long offset;
                if (this.lhs is RegisterRead)
                {
                    baseRegister = fill.GetRegister(0);
                    offset = fill.GetInt(1);
                }
                else
                {
                    offset = fill.GetInt(0);
                    baseRegister = fill.GetRegister(1);
                }

                return new LeaMovInstruction(result, baseRegister, offset);
            }

            private class LeaMovInstruction : Instruction
            {
                private readonly VirtualRegister result;
                private readonly VirtualRegister baseRegister;
                private readonly long offset;

                public LeaMovInstruction(
                    VirtualRegister result,
                    VirtualRegister baseRegister,
                    long offset)
                    : base(
                        new List<VirtualRegister> { baseRegister },
                        new List<VirtualRegister> { result })
                {
                    this.result = result;
                    this.baseRegister = baseRegister;
                    this.offset = offset;
                }

                public override IEnumerable<string> ToASM(
                    IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
                {
                    var resultHardware = this.result.ToHardware(registerAssignment);
                    var baseHardware = this.baseRegister.ToHardware(registerAssignment);
                    var sign = this.offset < 0 ? "-" : "+";
                    yield return $"mov {resultHardware}, [{baseHardware}{sign}{Math.Abs(this.offset)}]";
                }
            }
        }
    }
}