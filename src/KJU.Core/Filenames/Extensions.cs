namespace KJU.Core.Filenames
{
    using System.Text.RegularExpressions;

    public static class Extensions
    {
        public static string ChangeExtension(string input, string extension)
        {
            var basename = RemoveExtension(input);
            return $"{basename}.{extension}";
        }

        public static string RemoveExtension(string input)
        {
            return Regex.Replace(input, @"(\.[^.]*)*$", string.Empty);
        }
    }
}
