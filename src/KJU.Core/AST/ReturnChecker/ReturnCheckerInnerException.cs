namespace KJU.Core.AST.ReturnChecker
{
    using System;

    public class ReturnCheckerInnerException : Exception
    {
        public ReturnCheckerInnerException(string message)
            : base(message)
        {
        }
    }
}