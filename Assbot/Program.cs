using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assbot
{
    public class Program
    {
        static Bot bot;

        static bool Run = true;
        static void Main(string[] args)
        {
            bot = new Bot();

            while (Run)
            {
                string input = Console.ReadLine();

                switch (input)
                {
                    case "quit":
                        bot.Dispose();
                        Run = false;
                        break;
                    default:
                        bot.Send(input);
                        break;
                }
            }
        }
    }
}
