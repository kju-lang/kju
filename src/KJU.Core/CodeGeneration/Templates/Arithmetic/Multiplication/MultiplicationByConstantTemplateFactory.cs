#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Arithmetic.Multiplication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.AST;
    using KJU.Core.Intermediate;

    public class MultiplicationByConstantTemplateFactory : ITemplateFactory
    {
        public IList<InstructionTemplate> GetTemplates()
        {
            return new List<InstructionTemplate>
            {
                new ConstantMulTemplate(
                    new ArithmeticBinaryOperation(
                        ArithmeticOperationType.Multiplication,
                        null,
                        new IntegerImmediateValue(0) { TemplateValue = null })),
                new ConstantMulTemplate(
                    new ArithmeticBinaryOperation(
                        ArithmeticOperationType.Multiplication,
                        new IntegerImmediateValue(0) { TemplateValue = null },
                        null))
            };
        }

        private class ConstantMulTemplate : InstructionTemplate
        {
            private readonly ArithmeticBinaryOperation template;

            public ConstantMulTemplate(ArithmeticBinaryOperation template)
                : base(template, 5)
            {
                this.template = template;
            }

            public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
            {
                long constant;
                VirtualRegister register;

                if (this.template.Lhs is IntegerImmediateValue)
                {
                    constant = fill.GetInt(0);
                    register = fill.GetRegister(1);
                }
                else if (this.template.Rhs is IntegerImmediateValue)
                {
                    register = fill.GetRegister(0);
                    constant = fill.GetInt(1);
                }
                else
                {
                    throw new TemplateCreationException(
                        "Template does not have IntegerImmediateValue as a child. This should never happen.");
                }

                return new MulInstruction(register, constant, result);
            }
        }

        private class MulInstruction : Instruction
        {
            private readonly VirtualRegister input;
            private readonly long constant;
            private readonly VirtualRegister result;

            public MulInstruction(
                VirtualRegister input,
                long constant,
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
                this.constant = constant;
                this.result = result;
            }

            public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var inputHardware = this.input.ToHardware(registerAssignment);
                var resultHardware = this.result.ToHardware(registerAssignment);
                if (this.constant != (int)this.constant)
                {
                    return $"mov {resultHardware} {this.constant}\n" +
                           $"imul {resultHardware} {inputHardware}";
                }

                return $"imul {resultHardware} {inputHardware} {this.constant}\n";
            }
        }
    }
}