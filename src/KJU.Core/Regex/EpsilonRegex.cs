namespace KJU.Core.Regex
{
    public sealed class EpsilonRegex<Symbol> : Regex<Symbol>
    {
        public override bool Equals(object other)
        {
            return other is EpsilonRegex<Symbol>;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "EpsilonRegex";
        }
    }
}
