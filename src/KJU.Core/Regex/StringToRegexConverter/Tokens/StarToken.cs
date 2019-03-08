namespace KJU.Core.Regex.StringToRegexConverter.Tokens
{
    public class StarToken : Token
    {
        public override bool Equals(object obj)
        {
            return obj is StarToken;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "StarToken";
        }
    }
}