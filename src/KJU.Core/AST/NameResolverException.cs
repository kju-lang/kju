namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NameResolverException : Exception
    {
        public NameResolverException()
            : base()
        {
        }

        public NameResolverException(string message)
            : base(message)
        {
        }
    }
}
