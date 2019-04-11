#pragma warning disable SA1402 // File may only contain a single class
namespace KJU.Core.Intermediate
{
    public interface ILocation
    {
    }

    public class VirtualRegister : ILocation
    {
    }

    public class HardwareRegister : VirtualRegister
    {
        public HardwareRegister(HardwareRegisterName name)
        {
            this.Name = name;
        }

        public HardwareRegisterName Name { get; }
    }

    public class MemoryLocation : ILocation
    {
        public MemoryLocation(Function function, int offset)
        {
            this.Function = function;
            this.Offset = offset;
        }

        public Function Function { get; }

        public int Offset { get; }
    }
}