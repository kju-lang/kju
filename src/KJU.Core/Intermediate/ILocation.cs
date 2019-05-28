#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.Intermediate
{
    using System.Collections.Generic;

    public interface ILocation
    {
    }

    public class VirtualRegister : ILocation
    {
        private static int counter = 0;
        private readonly int number;

        public VirtualRegister()
        {
            this.number = counter++;
        }

        public override string ToString()
        {
            return $"r{this.number}";
        }
    }

    public class HardwareRegister : VirtualRegister
    {
        public static readonly HardwareRegister RAX = new HardwareRegister("RAX");

        public static readonly HardwareRegister RBX = new HardwareRegister("RBX");

        public static readonly HardwareRegister RCX = new HardwareRegister("RCX");

        public static readonly HardwareRegister RDX = new HardwareRegister("RDX");

        public static readonly HardwareRegister RBP = new HardwareRegister("RBP");

        public static readonly HardwareRegister RSP = new HardwareRegister("RSP");

        public static readonly HardwareRegister RSI = new HardwareRegister("RSI");

        public static readonly HardwareRegister RDI = new HardwareRegister("RDI");

        public static readonly HardwareRegister R8 = new HardwareRegister("R8");

        public static readonly HardwareRegister R9 = new HardwareRegister("R9");

        public static readonly HardwareRegister R10 = new HardwareRegister("R10");

        public static readonly HardwareRegister R11 = new HardwareRegister("R11");

        public static readonly HardwareRegister R12 = new HardwareRegister("R12");

        public static readonly HardwareRegister R13 = new HardwareRegister("R13");

        public static readonly HardwareRegister R14 = new HardwareRegister("R14");

        public static readonly HardwareRegister R15 = new HardwareRegister("R15");

        private HardwareRegister(string name)
        {
            this.Name = name;
        }

        public static IReadOnlyCollection<HardwareRegister> Values { get; } = new List<HardwareRegister>
        {
            RAX, RBX, RCX, RDX, RBP, RSP, RSI, RDI, R8, R9, R10, R11, R12, R13, R14, R15
        };

        public string Name { get; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class MemoryLocation : ILocation
    {
        public MemoryLocation(Function.Function function, int offset)
        {
            this.Function = function;
            this.Offset = offset;
        }

        public Function.Function Function { get; }

        public int Offset { get; }
    }

    public class HeapLocation : ILocation
    {
        public HeapLocation(Function.Function function, int offset, AST.DataType type)
        {
            this.Function = function;
            this.Offset = offset;
            this.Type = type;
        }

        public Function.Function Function { get; }

        public int Offset { get; }

        public AST.DataType Type { get; }
    }
}
