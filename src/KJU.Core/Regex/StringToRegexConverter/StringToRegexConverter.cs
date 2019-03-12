namespace KJU.Core.Regex.StringToRegexConverter
{
    public sealed class StringToRegexConverter : IStringToRegexConverter
    {
        private readonly IStringToTokensConverter stringToTokensConverter;
        private readonly IRegexTokensParser regexTokensParser;

        public StringToRegexConverter(
            IStringToTokensConverter stringToTokensConverter, IRegexTokensParser regexTokensParser)
        {
            this.stringToTokensConverter = stringToTokensConverter;
            this.regexTokensParser = regexTokensParser;
        }

        /// <summary>
        /// Converts strings containing characters (eg. a), character classes (eg. [a-z]), alternatives (|) ,stars (*) and round brackets
        /// to regex tree.
        /// Special symbols are escaped by backslash and also in character classes.
        /// </summary>
        /// <param name="regexString">string to parse</param>
        /// <returns>Root node of regex tree.</returns>
        /// <exception cref="RegexParseException">When input does not form correct regular expression.</exception>
        public Regex<char> Convert(string regexString)
        {
            var tokens = this.stringToTokensConverter.Convert(regexString);
            return this.regexTokensParser.Parse(tokens);
        }
    }
}