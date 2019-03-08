namespace KJU.Core.Regex.StringToRegexConverter
{
    public class StringToRegexConverterFactory : IStringToRegexConverterFactory
    {
        public IStringToRegexConverter CreateConverter()
        {
            var regexTokensParser = new RegexTokensParser();
            var stringToTokensConverter = new StringToTokensConverter();
            return new StringToRegexConverter(stringToTokensConverter, regexTokensParser);
        }
    }
}