namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using Intermediate;
    using Intermediate.Function;

    public static class InstructionsTemplatesUtils
    {
        public static HardwareRegister ToHardware(
            this VirtualRegister vr,
            IReadOnlyDictionary<VirtualRegister, HardwareRegister> registerAssignment)
        {
            return registerAssignment[vr];
        }

        public static VirtualRegister GetRegister(
            this IReadOnlyList<object> fill,
            int position)
        {
            return (VirtualRegister)fill[position];
        }

        public static long GetInt(
            this IReadOnlyList<object> fill,
            int position)
        {
            return (long)fill[position];
        }

        public static bool GetBool(
            this IReadOnlyList<object> fill,
            int position)
        {
            return (bool)fill[position];
        }

        public static string GetString(
            this IReadOnlyList<object> fill,
            int position)
        {
            return (string)fill[position];
        }

        public static Function GetFunction(
            this IReadOnlyList<object> fill,
            int position)
        {
            return (Function)fill[position];
        }
    }
}