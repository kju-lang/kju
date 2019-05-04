namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System.Collections.Generic;
    using Intermediate;
    using Intermediate.Function;

    public interface IFunctionToAsmGenerator
    {
        IEnumerable<string> ToAsm(Function function, ILabel label);
    }
}