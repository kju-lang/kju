namespace KJU.Core.Regex.StringToRegexConverter.Tokens
{
    public class RightBracketToken : Token
    {
        public override bool Equals(object obj)
        {
            return obj is RightBracketToken;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "RightBracketToken";
        }
    }
}