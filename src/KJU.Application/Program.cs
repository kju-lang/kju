using System;
using KJU.Core;

namespace KJU.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            KJU.Core.HelloWorld helloWorld = new KJU.Core.HelloWorld();
            Console.WriteLine(helloWorld.hello());
        }
    }
}
