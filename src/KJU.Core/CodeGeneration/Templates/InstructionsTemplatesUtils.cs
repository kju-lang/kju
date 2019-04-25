namespace KJU.Core.CodeGeneration.Templates
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

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
            var box = (IntegerImmediateValue)fill[position];
            return box.Value;
        }

        public static bool GetBool(
            this IReadOnlyList<object> fill,
            int position)
        {
            var box = (BooleanImmediateValue)fill[position];
            return box.Value;
        }

        public static Function GetFunction(
            this IReadOnlyList<object> fill,
            int position)
        {
            return (Function)fill[position];
        }
    }
}