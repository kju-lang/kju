namespace KJU.Core.Regex.StringToRegexConverter
{
    public interface IStringToRegexConverterFactory
    {
        IStringToRegexConverter CreateConverter();
    }
}