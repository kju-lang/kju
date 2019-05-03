namespace KJU.Core.Intermediate
{
    using System.Collections.Generic;

    public static class HardwareRegisterUtils
    {
        private static IReadOnlyList<HardwareRegister> argumentRegisters =
            new List<HardwareRegister>
            {
                HardwareRegister.RDI,
                HardwareRegister.RSI,
                HardwareRegister.RDX,
                HardwareRegister.RCX,
                HardwareRegister.R8,
                HardwareRegister.R9
            };

        private static IReadOnlyCollection<HardwareRegister> callerSavedRegisters =
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

        private static IReadOnlyCollection<HardwareRegister> calleeSavedRegisters =
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

        private static Dictionary<HardwareRegister, string> registerToEightBitsVersionMapping =
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

        public static IReadOnlyList<HardwareRegister> ArgumentRegisters { get => argumentRegisters; }

        public static IReadOnlyCollection<HardwareRegister> CallerSavedRegisters { get => callerSavedRegisters; }

        public static IReadOnlyCollection<HardwareRegister> CalleeSavedRegisters { get => calleeSavedRegisters; }

        public static string ToEightBitsVersion(this HardwareRegister register)
        {
            return registerToEightBitsVersionMapping[register];
        }
    }
}
