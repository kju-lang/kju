namespace KJU.Core.Filenames
{
    using System.Text.RegularExpressions;

    public static class Extensions
    {
        public static string ChangeExtension(this string input, string extension)
        {
            var basename = RemoveExtension(input);
            return $"{basename}.{extension}";
        }

        public static string RemoveExtension(this string input)
        {
            return Regex.Replace(input, @"(\.[^.]*)$", string.Empty);
        }

        public static string AddExtension(this string input, string extension)
        {
            return $"{input}.{extension}";
        }
    }
}