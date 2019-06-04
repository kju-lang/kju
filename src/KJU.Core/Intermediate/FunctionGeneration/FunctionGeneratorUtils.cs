namespace KJU.Core.Intermediate.FunctionGeneration
{
    using System;
    using AST.Types;

    internal static class FunctionGeneratorUtils
    {
        internal static int GetStackArgumentsCount(this Function.Function function)
        {
            return Math.Max(0, function.Parameters.Count + (function.Parent == null ? 0 : 1) - HardwareRegisterUtils.ArgumentRegisters.Count);
        }

        internal static int GetStackArgumentsCount(this FunType function)
        {
            return Math.Max(0, function.ArgTypes.Count + 1 - HardwareRegisterUtils.ArgumentRegisters.Count);
        }
    }
}
