namespace KJU.Core.Regex.StringToRegexConverter
{
    using System.Collections.Generic;
    using KJU.Core.Regex.StringToRegexConverter.Tokens;

    public interface IStringToTokensConverter
    {
        List<Token> Convert(string regexString);
    }
}