namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;

    public class TypeCheckerException : AggregateException
    {
        public TypeCheckerException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions)
        {
        }

        public TypeCheckerException(string message, params Exception[] innerExceptions)
            : base(message, innerExceptions)
        {
        }
    }
}