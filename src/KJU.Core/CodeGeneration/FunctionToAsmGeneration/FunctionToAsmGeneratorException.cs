namespace KJU.Core.CodeGeneration.FunctionToAsmGeneration
{
    using System;

    public class FunctionToAsmGeneratorException : Exception
    {
        public FunctionToAsmGeneratorException(string message)
            : base(message)
        {
        }
    }
}