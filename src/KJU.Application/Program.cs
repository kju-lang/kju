namespace KJU.Application
{
    using System;
    using KJU.Core;

    /// <summary>
    /// Main application class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point for the application.
        /// </summary>
        /// <param name="args">A list of command line arguments.</param>
        public static void Main(string[] args)
        {
            KJU.Core.HelloWorld helloWorld = new KJU.Core.HelloWorld();
            Console.WriteLine(helloWorld.Hello());
        }
    }
}
