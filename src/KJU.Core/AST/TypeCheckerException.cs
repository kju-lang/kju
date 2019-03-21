namespace KJU.Core.AST
{
    using System;

    public class TypeCheckerException : Exception
    {
        public TypeCheckerException(string message)
            : base(message)
        {
        }
    }
}