namespace KJU.Core.Intermediate
{
    using System.Collections.Generic;

    public static class HardwareRegisterUtils
    {
        public static IReadOnlyList<HardwareRegister> ArgumentRegisters()
        {
            return new List<HardwareRegister>
            {
                HardwareRegister.RDI,
                HardwareRegister.RSI,
                HardwareRegister.RDX,
                HardwareRegister.RCX,
                HardwareRegister.R8,
                HardwareRegister.R9
            };
        }

        public static IReadOnlyCollection<HardwareRegister> CallerSavedRegisters()
        {
            return new List<HardwareRegister>
            {
                HardwareRegister.RAX,
                HardwareRegister.RCX,
                HardwareRegister.RDX,
                HardwareRegister.RSI,
                HardwareRegister.RDI,
                HardwareRegister.R8,
                HardwareRegister.R9,
                HardwareRegister.R10,
                HardwareRegister.R11,
            };
        }

        public static IReadOnlyCollection<HardwareRegister> CalleeSavedRegisters()
        {
            return new List<HardwareRegister>
            {
                HardwareRegister.RBX,
                HardwareRegister.RSP,
                HardwareRegister.RBP,
                HardwareRegister.R12,
                HardwareRegister.R13,
                HardwareRegister.R14,
                HardwareRegister.R15,
            };
        }
    }
}
