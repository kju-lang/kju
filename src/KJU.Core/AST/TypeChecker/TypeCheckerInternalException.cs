namespace KJU.Core.AST
{
    using System;

    public class TypeCheckerInternalException : Exception
    {
        public TypeCheckerInternalException(string message)
            : base(message)
        {
        }
    }
}