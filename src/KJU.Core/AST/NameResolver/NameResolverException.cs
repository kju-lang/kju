namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public class NameResolverException : AggregateException
    {
        public NameResolverException()
        {
        }

        public NameResolverException(IEnumerable<Exception> innerExceptions)
            : base(innerExceptions)
        {
        }

        public NameResolverException(params Exception[] innerExceptions)
            : base(innerExceptions)
        {
        }

        public NameResolverException(string message)
            : base(message)
        {
        }

        public NameResolverException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions)
        {
        }

        public NameResolverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NameResolverException(string message, params Exception[] innerExceptions)
            : base(message, innerExceptions)
        {
        }
    }
}