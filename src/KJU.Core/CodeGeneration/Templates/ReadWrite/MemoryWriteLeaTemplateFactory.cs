namespace KJU.Core.CodeGeneration.Templates.ReadWrite
{
    using System;
    using System.Collections.Generic;
    using AST;
    using Intermediate;

    internal class MemoryWriteLeaTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return new List<InstructionTemplate>
            {
                new MemoryWriteLeaTemplate(new RegisterRead(null), new IntegerImmediateValue()),
                new MemoryWriteLeaTemplate(new IntegerImmediateValue(), new RegisterRead(null))
            };
        }

        private class MemoryWriteLeaTemplate : InstructionTemplate
        {
            private readonly Intermediate.Node lhs;
            private readonly Intermediate.Node rhs;

            public MemoryWriteLeaTemplate(Intermediate.Node lhs, Intermediate.Node rhs)
                : base(
                    new MemoryWrite(
                        new ArithmeticBinaryOperation(
                            ArithmeticOperationType.Addition,
                            lhs,
                            rhs),
                        null),
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

                var value = fill.GetRegister(2);
                return new LeaMovInstruction(baseRegister, offset, value);
            }

            private class LeaMovInstruction : Instruction
            {
                private readonly VirtualRegister baseRegister;
                private readonly long offset;
                private readonly VirtualRegister value;

                public LeaMovInstruction(
                    VirtualRegister baseRegister,
                    long offset,
                    VirtualRegister value)
                    : base(new List<VirtualRegister> { value, baseRegister }) // No register is written to. Only memory.
                {
                    this.baseRegister = baseRegister;
                    this.offset = offset;
                    this.value = value;
                }

                public override IEnumerable<string> ToASM(
                    IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
                {
                    var baseHardware = this.baseRegister.ToHardware(registerAssignment);
                    var fromHardware = this.value.ToHardware(registerAssignment);
                    var sign = this.offset < 0 ? "-" : "+";
                    yield return $"mov [{baseHardware}{sign}{Math.Abs(this.offset)}], {fromHardware}";
                }
            }
        }
    }
}