namespace KJU.Core.Intermediate.FunctionGeneration
{
    using System;

    internal static class FunctionGeneratorUtils
    {
        internal static int GetStackArgumentsCount(this Function.Function function)
        {
            return Math.Max(0, function.Parameters.Count + (function.Parent == null ? 0 : 1) - HardwareRegisterUtils.ArgumentRegisters.Count);
        }
    }
}
