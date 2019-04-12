#pragma warning disable SA1008 // Opening parenthesis must not be followed by a space.
#pragma warning disable SA1118  // Parameter must not span multiple lines
namespace KJU.Core.CodeGeneration.Templates.Comparison
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Intermediate;

    public class ComparisonInstruction : Instruction
    {
        private const int LabelLength = 20;

        private readonly VirtualRegister lhs;
        private readonly VirtualRegister rhs;
        private readonly VirtualRegister result;
        private readonly string jumpInstruction;

        public ComparisonInstruction(
            VirtualRegister lhs,
            VirtualRegister rhs,
            VirtualRegister result,
            string jumpInstruction)
            : base(
                new List<VirtualRegister> { lhs, rhs },
                new List<VirtualRegister> { result },
                new List<Tuple<VirtualRegister, VirtualRegister>>())
        {
            this.lhs = lhs;
            this.rhs = rhs;
            this.result = result;
            this.jumpInstruction = jumpInstruction;
        }

        public override string ToASM(IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            var lhsHardware = this.lhs.ToHardware(registerAssignment);
            var rhsHardware = this.rhs.ToHardware(registerAssignment);
            var resultHardware = this.result.ToHardware(registerAssignment);
            var successLabelName = GenerateRandomLabelName(LabelLength);
            var exitLabelName = GenerateRandomLabelName(LabelLength);

            var builder = new StringBuilder();
            builder.AppendLine($"cmp {lhsHardware} {rhsHardware}");
            builder.AppendLine($"{this.jumpInstruction} {successLabelName}");
            builder.AppendLine($"mov {resultHardware} 0");
            builder.AppendLine($"jmp {exitLabelName}");
            builder.AppendLine($"{successLabelName}:");
            builder.AppendLine($"mov {resultHardware} 1");
            builder.AppendLine($"{exitLabelName}:");

            return builder.ToString();
        }

        private static string GenerateRandomLabelName(int length)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var alphabet = Enumerable.Range('a', 26).Select(x => (char)x).ToList();
            return new string(Enumerable.Repeat(alphabet, length).Select(s => s[random.Next(s.Count)]).ToArray());
        }
    }
}