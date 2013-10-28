using System;
using System.Reflection;
using System.Threading;

namespace Assbot
{
	public class Program
	{
		public static void Main(string[] args)
		{
#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => Console.WriteLine(eventArgs.ExceptionObject);
#endif

			Assembly assembly = Assembly.GetExecutingAssembly();
			Console.WriteLine("Assbot v{0}", assembly.GetName().Version);

			Bot bot = new Bot();

			Thread botThread = new Thread(
				() =>
				{
					if (!bot.Connect(Configuration.Server))
					{
						Console.WriteLine("Cannot connect to {0}!", Configuration.Server);
						Console.ReadKey();
						return;
					}

					if (!bot.JoinChannel(Configuration.Channel))
					{
						Console.WriteLine("Cannot join channel {0}!", Configuration.Channel);
						Console.ReadKey();
						return;
					}

					while(bot.IsRunning)
						Thread.Sleep(1);

					bot.Shutdown();
				});

			botThread.Start();

			while(botThread.IsAlive)
			{
				if (!bot.IsInChannel || !bot.IsIdentified)
				{
					Thread.Sleep(1);
					continue;
				}

				Console.Write("> ");
				string command = Console.ReadLine();

				switch(command)
				{
					case "quit":
						Console.WriteLine("Shutting down...");
						bot.Quit("Shutdown from console.");

						while(botThread.IsAlive)
							Thread.Sleep(1);
						break;
				}
			}
		}
	}
}
