#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Arithmetic
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AST;
    using Intermediate;

    internal class DivTemplate : InstructionTemplate
    {
        public DivTemplate()
            : base(new ArithmeticBinaryOperation(ArithmeticOperationType.Division, null, null), 1)
        {
        }

        public override Instruction Emit(VirtualRegister result, IReadOnlyList<object> fill, string label)
        {
            var lhs = fill.GetRegister(0);
            var rhs = fill.GetRegister(1);
            return new DivInstruction(lhs, rhs, result);
        }

        private class DivInstruction : Instruction
        {
            private readonly VirtualRegister lhs;
            private readonly VirtualRegister rhs;
            private readonly VirtualRegister result;

            public DivInstruction(
                VirtualRegister lhs,
                VirtualRegister rhs,
                VirtualRegister result)
                : base(
                    new List<VirtualRegister> { lhs, rhs, HardwareRegister.RDX, HardwareRegister.RAX },
                    new List<VirtualRegister>
                    {
                        result,
                        HardwareRegister.RAX,
                        HardwareRegister.RDX
                    }, new List<Tuple<VirtualRegister, VirtualRegister>>
                    {
                        new Tuple<VirtualRegister, VirtualRegister>(HardwareRegister.RAX, lhs),
                        new Tuple<VirtualRegister, VirtualRegister>(HardwareRegister.RAX, result),
                    })
            {
                this.lhs = lhs;
                this.rhs = rhs;
                this.result = result;
            }

            public override IEnumerable<string> ToASM(
                IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
            {
                var lhsHardware = this.lhs.ToHardware(registerAssignment);
                var rhsHardware = this.rhs.ToHardware(registerAssignment);
                var resultHardware = this.result.ToHardware(registerAssignment);
                var rax = HardwareRegister.RAX;
                var rdx = HardwareRegister.RDX;

                if (rax != lhsHardware)
                {
                    yield return $"mov {rax}, {lhsHardware}";
                }

                yield return $"mov {rdx}, 0";
                yield return $"idiv {rhsHardware}";

                if (rax != resultHardware)
                {
                    yield return $"mov {resultHardware}, {rax}";
                }
            }
        }
    }
}