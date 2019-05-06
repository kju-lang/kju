namespace KJU.Core.Intermediate
{
    using System.Collections.Generic;

    public static class HardwareRegisterUtils
    {
        private static readonly Dictionary<HardwareRegister, string> RegisterToEightBitsVersionMapping =
            new Dictionary<HardwareRegister, string>
            {
                [HardwareRegister.RAX] = "AL",
                [HardwareRegister.RBX] = "BL",
                [HardwareRegister.RCX] = "CL",
                [HardwareRegister.RDX] = "DL",
                [HardwareRegister.RSI] = "SIL",
                [HardwareRegister.RDI] = "DIL",
                [HardwareRegister.RBP] = "BPL",
                [HardwareRegister.RSP] = "SPL",
                [HardwareRegister.R8] = "R8B",
                [HardwareRegister.R9] = "R9B",
                [HardwareRegister.R10] = "R10B",
                [HardwareRegister.R11] = "R11B",
                [HardwareRegister.R12] = "R12B",
                [HardwareRegister.R13] = "R13B",
                [HardwareRegister.R14] = "R14B",
                [HardwareRegister.R15] = "R15B"
            };

        public static IReadOnlyList<HardwareRegister> ArgumentRegisters { get; } =
            new List<HardwareRegister>
            {
                HardwareRegister.RDI,
                HardwareRegister.RSI,
                HardwareRegister.RDX,
                HardwareRegister.RCX,
                HardwareRegister.R8,
                HardwareRegister.R9
            };

        public static IReadOnlyList<HardwareRegister> CallerSavedRegisters { get; } =
            new List<HardwareRegister>
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

        public static IReadOnlyCollection<HardwareRegister> CalleeSavedRegisters { get; } =
            new List<HardwareRegister>
            {
                HardwareRegister.RBX,
                HardwareRegister.RSP,
                HardwareRegister.RBP,
                HardwareRegister.R12,
                HardwareRegister.R13,
                HardwareRegister.R14,
                HardwareRegister.R15,
            };

        public static IReadOnlyCollection<HardwareRegister> RegistersForColoring { get; } =
            new List<HardwareRegister>
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
                HardwareRegister.RBX,
                HardwareRegister.R12,
                HardwareRegister.R13,
                HardwareRegister.R14,
                HardwareRegister.R15,
            };

        public static string ToEightBitsVersion(this HardwareRegister register)
        {
            return RegisterToEightBitsVersionMapping[register];
        }
    }
}