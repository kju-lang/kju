namespace KJU.Core.AST.ReturnChecker
{
    using System;

    public class ExpressionEvaluatorException : Exception
    {
        public ExpressionEvaluatorException(string message)
            : base(message)
        {
        }
    }
}