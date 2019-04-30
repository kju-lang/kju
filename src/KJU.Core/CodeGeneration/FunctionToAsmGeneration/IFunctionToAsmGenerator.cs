namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System.Collections.Generic;
    using Intermediate;
    using KJU.Core.AST;

    public interface IFunctionToAsmGenerator
    {
        IEnumerable<string> ToAsm(FunctionDeclaration functionDeclaration);
    }
}