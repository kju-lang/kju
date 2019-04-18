namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System.Collections.Generic;
    using Intermediate;

    public interface IFunctionToAsmGenerator
    {
        IEnumerable<string> ToAsm(IFunction function);
    }
}