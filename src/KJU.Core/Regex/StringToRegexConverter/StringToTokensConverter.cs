namespace KJU.Core.Regex.StringToRegexConverter
{
    using System.Collections.Generic;
    using System.Linq;
    using Tokens;

    public class StringToTokensConverter : IStringToTokensConverter
    {
        public List<Token> Convert(string regexString)
        {
            var result = new List<Token>();
            var currentCharIndex = 0;
            var length = regexString.Length;
            while (currentCharIndex != length)
            {
                var currentChar = regexString[currentCharIndex];
                Token toAdd;
                switch (currentChar)
                {
                    case '(':
                        toAdd = new LeftBracketToken();
                        break;
                    case ')':
                        toAdd = new RightBracketToken();
                        break;
                    case '*':
                        toAdd = new StarToken();
                        break;
                    case '|':
                        toAdd = new SumToken();
                        break;
                    case '\\':
                        currentCharIndex++;
                        if (currentCharIndex == length)
                        {
                            throw new RegexParseException($"Backslash at the end of input escaping nothing.");
                        }

                        toAdd = new CharacterClassToken($"\\{regexString[currentCharIndex]}");
                        break;
                    case '[':
                        currentCharIndex++;
                        var startingIndex = currentCharIndex;
                        while (currentCharIndex != length && regexString[currentCharIndex] != ']')
                        {
                            // Escape inside character class
                            if ('\\'.Equals(regexString[currentCharIndex]))
                            {
                                currentCharIndex++;
                            }

                            currentCharIndex++;
                        }

                        if (currentCharIndex == length)
                        {
                            throw new RegexParseException($"Character class not properly closed in {regexString}");
                        }

                        var value = regexString.Substring(startingIndex, currentCharIndex - startingIndex);
                        toAdd = new CharacterClassToken(value);
                        break;

                    default:
                        toAdd = new CharacterClassToken($"\\{currentChar}");
                        break;
                }

                result.Add(toAdd);
                currentCharIndex++;
            }

            return result;
        }
    }
}