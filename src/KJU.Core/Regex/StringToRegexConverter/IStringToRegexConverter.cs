namespace KJU.Core.Regex.StringToRegexConverter
{
    public interface IStringToRegexConverter
    {
        /// <summary>
        /// Converts strings containing characters (eg. a), character classes (eg. [a-z]), alternatives (|) ,stars (*) and round brackets
        /// to regex tree.
        /// Special symbols are escaped by backslash and also in character classes.
        /// </summary>
        /// <param name="regexString">string to parse</param>
        /// <returns>Root node of regex tree.</returns>
        /// <exception cref="RegexParseException">When input does not form correct regular expression.</exception>
        Regex<char> Convert(string regexString);
    }
}