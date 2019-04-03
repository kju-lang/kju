namespace KJU.Core.AST.ReturnChecker
{
    using System;
    using System.Collections.Generic;

    public class ReturnCheckerException : AggregateException
    {
        public ReturnCheckerException(string message, IEnumerable<Exception> innerExceptions)
            : base(message, innerExceptions)
        {
        }
    }
}