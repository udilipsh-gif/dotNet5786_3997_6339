using System;
namespace stage0
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Welcome3997();
            Welcome6339();

        }
        static partial void Welcome6339();
        private static void Welcome3997()
        {
            Console.WriteLine("Hello, World!");
            Console.Write("Enter your name: ");
            string? name = Console.ReadLine();
            Console.WriteLine("{0}, welcome to the first application!", name);
        }
    }
}


