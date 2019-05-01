namespace KJU.Core.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public class InstructionSelectorException : AggregateException
    {
        public InstructionSelectorException()
        {
        }

        public InstructionSelectorException(IEnumerable<Exception> innerExceptions)
            : base(innerExceptions)
        {
        }

        public InstructionSelectorException(params Exception[] innerExceptions)
            : base(innerExceptions)
        {
        }

        public InstructionSelectorException(string message)
            : base(message)
        {
        }

        public InstructionSelectorException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions)
        {
        }

        public InstructionSelectorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstructionSelectorException(string message, params Exception[] innerExceptions)
            : base(message, innerExceptions)
        {
        }
    }
}
