namespace KJU.Core.Lexer
{
    using System;

    public class LexerException : Exception
    {
        public LexerException(string message)
            : base(message)
        {
        }
    }
}