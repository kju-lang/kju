#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Arithmetic.Multiplication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.AST;
    using KJU.Core.Intermediate;

    public class PowerOf2MultiplicationTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return Enumerable.Range(1, 62).SelectMany(power => new List<InstructionTemplate>
            {
                new Pow2MulTemplate(
                    new ArithmeticBinaryOperation(
                        ArithmeticOperationType.Multiplication,
                        null,
                        new IntegerImmediateValue(0) { TemplateValue = 1L << power }), power),
                new Pow2MulTemplate(
                    new ArithmeticBinaryOperation(
                        ArithmeticOperationType.Multiplication,
                        new IntegerImmediateValue(0) { TemplateValue = 1L << power },
                        null), power),
            }).ToList();
        }

        private class Pow2MulTemplate : InstructionTemplate
        {
            private readonly int power;

            public Pow2MulTemplate(Intermediate.Node template, int power)
                : base(template, 10)
            {
                this.power = power;
            }

            public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
            {
                var input = fill.GetRegister(0);
                return new MulInstruction(input, this.power, result);
            }
        }

        private class MulInstruction : Instruction
        {
            private readonly VirtualRegister input;
            private readonly int power;
            private readonly VirtualRegister result;

            public MulInstruction(
                VirtualRegister input,
                int power,
                VirtualRegister result)
                : base(
                    new List<VirtualRegister> { input },
                    new List<VirtualRegister> { result },
                    new List<Tuple<VirtualRegister, VirtualRegister>>
                    {
                        new Tuple<VirtualRegister, VirtualRegister>(input, result),
                    })
            {
                this.input = input;
                this.power = power;
                this.result = result;
            }

            public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var inputHardware = this.input.ToHardware(registerAssignment);
                var resultHardware = this.result.ToHardware(registerAssignment);

                var builder = new StringBuilder();
                if (!resultHardware.Equals(inputHardware))
                {
                    builder.AppendLine($"mov {resultHardware}, {inputHardware}");
                }

                builder.Append($"shl {resultHardware}, {this.power}");
                return builder.ToString();
            }
        }
    }
}