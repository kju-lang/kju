namespace KJU.Core.Regex.StringToRegexConverter
{
    using System.Collections.Generic;
    using Tokens;

    public interface IRegexTokensParser
    {
        Regex Parse(List<Token> tokensToParse);
    }
}