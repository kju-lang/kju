namespace KJU.Core.AST
{
    using System;

    public class ParseTreeToAstConverterException : Exception
    {
        public ParseTreeToAstConverterException(string msg)
            : base(msg)
        {
        }
    }
}