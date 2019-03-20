namespace KJU.Application
{
    using System;
    using System.IO;
    using KJU.Core;
    using KJU.Core.Parser;

    public class Program
    {
        public static void Main(string[] args)
        {
            foreach (string filename in args)
            {
                string data = File.ReadAllText(filename);
                Console.WriteLine($"compiling {data}...");
                var tree = KjuParserFactory.Instance.Parse(data);
                Console.WriteLine($"tree: {tree}");
            }
        }
    }
}
