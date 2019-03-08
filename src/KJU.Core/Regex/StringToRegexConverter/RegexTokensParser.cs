namespace KJU.Core.Regex.StringToRegexConverter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tokens;

    public class RegexTokensParser : IRegexTokensParser
    {
        private List<Token> tokens;

        /// <summary>
        /// Number of tokens successfully parsed.
        /// This number can decrease if parser is backtracking.
        /// </summary>
        private int alreadyParsed;

        public Regex Parse(List<Token> tokensToParse)
        {
            this.tokens = tokensToParse;
            this.alreadyParsed = 0;
            try
            {
                return this.ParseAll();
            }
            catch (RegexParserInternalException e)
            {
                throw new RegexParseException("Parsing failed.", e);
            }
        }

        private bool Accept(Type type)
        {
            if (this.alreadyParsed == this.tokens.Count)
            {
                return false;
            }

            if (this.tokens[this.alreadyParsed].GetType() == type)
            {
                this.alreadyParsed++;
                return true;
            }

            return false;
        }

        private void Expect(Type expectType)
        {
            if (this.alreadyParsed == this.tokens.Count)
            {
                throw new RegexParserInternalException($"Expect fail. Needed: {expectType}, got: end of input");
            }

            var currentType = this.tokens[this.alreadyParsed].GetType();
            if (currentType != expectType)
            {
                throw new RegexParserInternalException($"Expect fail. Needed: {expectType}, got: {currentType}");
            }

            this.alreadyParsed++;
        }

        private Regex ParseAll()
        {
            var alreadyParsedBackup = this.alreadyParsed;
            try
            {
                var result = this.ParseWithoutSum();
                while (this.Accept(typeof(SumToken)))
                {
                    var other = this.ParseWithoutSum();
                    result = new SumRegex(result, other);
                }

                return result;
            }
            catch (RegexParserInternalException)
            {
                this.alreadyParsed = alreadyParsedBackup;
                throw;
            }
        }

        private Regex ParseWithoutSum()
        {
            var alreadyParsedBackup = this.alreadyParsed;
            try
            {
                var result = this.ParseWithoutSumAndCatenation();
                try
                {
                    while (true)
                    {
                        var other = this.ParseWithoutSumAndCatenation();
                        if (other is EpsilonRegex)
                        {
                            break;
                        }

                        result = new ConcatRegex(result, other);
                    }
                }
                catch (RegexParserInternalException)
                {
                    // Concat do not require special character,
                    // so parser does not know if it should stop parsing or not.
                    // Therefore fail in this branch does not necessarily means global fail.
                }

                return result;
            }
            catch (RegexParserInternalException)
            {
                this.alreadyParsed = alreadyParsedBackup;
                throw;
            }
        }

        private Regex ParseWithoutSumAndCatenation()
        {
            var alreadyParsedBackup = this.alreadyParsed;
            try
            {
                var result = this.ParseWithoutSumCatenationAndStar();
                while (this.Accept(typeof(StarToken)))
                {
                    result = new StarRegex(result);
                }

                return result;
            }
            catch (RegexParserInternalException)
            {
                this.alreadyParsed = alreadyParsedBackup;
                throw;
            }
        }

        private Regex ParseWithoutSumCatenationAndStar()
        {
            var alreadyParsedBackup = this.alreadyParsed;
            try
            {
                if (this.Accept(typeof(LeftBracketToken)))
                {
                    var result = this.ParseAll();
                    this.Expect(typeof(RightBracketToken));
                    return result;
                }

                return this.ParseCharacterClass();
            }
            catch (RegexParserInternalException)
            {
                this.alreadyParsed = alreadyParsedBackup;
                throw;
            }
        }

        private Regex ParseCharacterClass()
        {
            if (!this.Accept(typeof(CharacterClassToken)))
            {
                return new EpsilonRegex();
            }

            var token = (CharacterClassToken)this.tokens[this.alreadyParsed - 1];
            return this.CreateRegexFromCharacterClassToken(token);
        }

        private Regex CreateRegexFromCharacterClassToken(CharacterClassToken token)
        {
            var chars = token.Value;
            var regexes = new List<Regex>();
            for (var index = 0; index < chars.Length; index++)
            {
                var currentChar = chars[index];
                switch (currentChar)
                {
                    case '\\':
                        index++;
                        if (index == chars.Length)
                        {
                            throw new RegexParserInternalException("Character class: escape (\\) at the end of body.");
                        }

                        regexes.Add(new AtomicRegex(chars[index]));
                        break;
                    case '-':
                        var previousIndex = index - 1;
                        var nextIndex = index + 1;
                        if (nextIndex == chars.Length)
                        {
                            throw new RegexParserInternalException(
                                $"Character class: unescaped minus at the end of body: {chars}");
                        }

                        if ('\\'.Equals(chars[nextIndex]))
                        {
                            nextIndex++;
                        }

                        if (nextIndex == chars.Length)
                        {
                            throw new RegexParserInternalException(
                                $"Character class: backslash after minus at the end of body: {chars}");
                        }

                        if (previousIndex < 0)
                        {
                            throw new RegexParserInternalException(
                                $"Character class: unescaped minus at the beginning of body: {chars}");
                        }

                        regexes.RemoveAt(regexes.Count - 1);

                        var startCharacter = chars[previousIndex];
                        var endCharacter = chars[nextIndex];
                        if (startCharacter > endCharacter)
                        {
                            throw new RegexParserInternalException(
                                $"Character class: start character > end character: {startCharacter} > {endCharacter}");
                        }

                        for (var charToAdd = startCharacter; charToAdd <= endCharacter; charToAdd++)
                        {
                            regexes.Add(new AtomicRegex(charToAdd));
                        }

                        index = nextIndex;
                        break;
                    default:
                        regexes.Add(new AtomicRegex(currentChar));
                        break;
                }
            }

            if (regexes.Count == 0)
            {
                return new EmptyRegex();
            }

            var first = regexes[0];
            regexes.RemoveAt(0);
            return regexes.Aggregate(first, (acc, x) => new SumRegex(acc, x));
        }
    }
}