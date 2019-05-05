namespace KJU.Core.CodeGeneration.Templates
{
    using System;

    public class InstructionException : Exception
    {
        public InstructionException(string message)
            : base(message)
        {
        }
    }
}