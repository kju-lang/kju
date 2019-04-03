namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NameResolverInternalException : Exception
    {
        public NameResolverInternalException()
            : base()
        {
        }

        public NameResolverInternalException(string message)
            : base(message)
        {
        }
    }
}
