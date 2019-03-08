namespace KJU.Core.Regex.StringToRegexConverter.Tokens
{
    public class LeftBracketToken : Token
    {
        public override bool Equals(object obj)
        {
            return obj is LeftBracketToken;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "LeftBracketToken";
        }
    }
}