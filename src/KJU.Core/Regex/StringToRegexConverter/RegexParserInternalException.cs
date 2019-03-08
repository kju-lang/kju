namespace KJU.Core.Regex.StringToRegexConverter
{
    using System;

    public class RegexParserInternalException : Exception
    {
        public RegexParserInternalException()
        {
        }

        public RegexParserInternalException(string message)
            : base(message)
        {
        }

        public RegexParserInternalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}