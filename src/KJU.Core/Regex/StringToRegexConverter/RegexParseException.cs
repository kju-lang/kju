namespace KJU.Core.Regex.StringToRegexConverter
{
    using System;

    public class RegexParseException : Exception
    {
        public RegexParseException()
        {
        }

        public RegexParseException(string message)
            : base(message)
        {
        }

        public RegexParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}