namespace KJU.Core.Visualization
{
    using System.Text.RegularExpressions;

    public static class Dot
    {
        public static string Escape(string input)
        {
            return new Regex("([\"])").Replace(input, "\\$1");
        }
    }
}
