namespace KJU.Core.Regex.StringToRegexConverter.Tokens
{
    public class SumToken : Token
    {
        public override bool Equals(object obj)
        {
            return obj is SumToken;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "SumToken";
        }
    }
}