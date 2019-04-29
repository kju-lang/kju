namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System.Collections.Generic;
    using AST;

    public interface IFunctionToAsmGenerator
    {
        IEnumerable<string> ToAsm(FunctionDeclaration functionDeclaration);
    }
}