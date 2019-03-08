namespace KJU.Core.Regex.StringToRegexConverter.Tokens
{
    public class CharacterClassToken : Token
    {
        public CharacterClassToken(string value)
        {
            this.Value = value;
        }

        public string Value { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is CharacterClassToken))
            {
                return false;
            }

            var otherCharacterClassToken = (CharacterClassToken)obj;
            return this.Value.Equals(otherCharacterClassToken.Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"CharacterClassToken{{{this.Value}}}";
        }
    }
}