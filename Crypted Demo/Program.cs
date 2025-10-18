using System;

namespace Crypted_Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Type \"run\" to connect to server, decrypt SnakeGame's methods, and play Console Snake!");
                Console.WriteLine("Type \"exit\" to exit.");
                string input = "";

                do input = Console.ReadLine();
                while ((input.ToLower() != "run") && (input.ToLower() != "exit"));

                if (input.ToLower() == "run")
                {
                    var sc = new SimpleClient("127.0.0.1", 9000);
                    sc.RunClient();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
            }
        }
    }
}
