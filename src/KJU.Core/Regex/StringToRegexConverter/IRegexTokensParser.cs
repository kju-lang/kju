namespace KJU.Core.Regex.StringToRegexConverter
{
    using System.Collections.Generic;
    using Tokens;
    using Regex = Regex<char>;

    public interface IRegexTokensParser
    {
        Regex<char> Parse(List<Token> tokensToParse);
    }
}