namespace KJU.Core.Intermediate
{
    using System;

    public class FunctionObjectException : Exception
    {
        public FunctionObjectException()
            : base()
        {
        }

        public FunctionObjectException(string message)
            : base(message)
        {
        }
    }
}
