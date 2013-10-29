using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using IrcDotNet;

namespace Assbot
{
	public class Bot
	{
		public bool IsRunning { get; private set; }

		//public bool IsRunning
		//{
		//	get
		//	{
		//		return client.IsConnected;
		//	}
		//}

		public bool IsInChannel { get; private set; }
		public bool IsIdentified { get; private set; }

		private readonly IrcClient client;
		private static IrcUserRegistrationInfo RegistrationInfo
		{
			get
			{
				return new IrcUserRegistrationInfo
				{
					NickName = Configuration.Username,
					UserName = Configuration.Username,
					RealName = "Assbot"
				};
			}
		}

		private readonly List<Command> commands;

		// TODO Terrible hack
		private static readonly object CommonLock;

		static Bot()
		{
			CommonLock = new object();
		}

		public Bot()
		{
			commands = Command.GetCommands(this);
			IsInChannel = false;
			IsIdentified = false;
			IsRunning = true;

			client = new IrcClient
			{
				FloodPreventer = new IrcStandardFloodPreventer(4, 2000)
			};

			client.Connected += (sender, args) => Console.WriteLine("Connected!");
			client.Disconnected += (sender, args) =>
			{
				const int MaxRetries = 16;
				int tries = 0;

				IsInChannel = false;
				IsIdentified = false;

				Console.WriteLine("Lost connection, reconnecting...");

				// Reconnect
				Console.Write("Reconnecting... ");
				while (!Connect(Configuration.Server) && tries++ < MaxRetries)
					Thread.Sleep(1500);

				if (tries == MaxRetries)
				{
					Console.WriteLine("Failed.");
					Quit("Failed to reconnect.");
					return;
				}
				
				Console.WriteLine("Connected");
				Console.Write("Joining channel... ");

				if (JoinChannel(Configuration.Channel))
					Console.WriteLine("Success");
				else
				{
					Console.WriteLine("Failed");
					Quit("Failed to rejoin channel.");
				}
			};

			client.Registered += (sender, args) =>
			{
				IrcClient localClient = (IrcClient)sender;

				// Identify with server
				client.SendRawMessage(String.Format("ns identify {0}", Configuration.Password));

				localClient.LocalUser.NoticeReceived += (o, eventArgs) =>
				{
					if (eventArgs.Text.StartsWith("Password accepted"))
						IsIdentified = true;

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine(eventArgs.Text);
					Console.ForegroundColor = ConsoleColor.Gray;
				};

				localClient.LocalUser.JoinedChannel += (o, eventArgs) =>
				{
					IrcChannel channel = localClient.Channels.FirstOrDefault();
					if (channel == null)
						return;

					Console.WriteLine("Joined channel!");

					channel.MessageReceived += HandleMessage;
					channel.UserJoined += (sender1, userEventArgs) =>
					{
						string joinMessage = String.Format("Used joined: {0}", userEventArgs.Comment ?? "No comment");

						foreach (Command command in commands)
							command.HandlePassive(joinMessage, userEventArgs.ChannelUser.User.NickName);
					};

					channel.UserLeft += (sender1, userEventArgs) =>
					{
						string leftMessage = String.Format("Used left: {0}", userEventArgs.Comment ?? "No comment");

						foreach (Command command in commands)
							command.HandlePassive(leftMessage, userEventArgs.ChannelUser.User.NickName);
					};

					IsInChannel = true;
				};

				localClient.LocalUser.LeftChannel += (o, eventArgs) =>
				{
					Console.Write("Rejoining channel... ");
					if (JoinChannel(Configuration.Channel))
						Console.WriteLine("Success");
					else
					{
						Console.WriteLine("Failed");
						Quit("Failed to rejoin channel.");
					}
				};

				Console.WriteLine("Registered!");
			};
		}

		public bool Connect(string server)
		{
			ManualResetEventSlim connectedEvent = new ManualResetEventSlim(false);
			client.Connected += (sender, e) => connectedEvent.Set();
			client.Connect(server, false, RegistrationInfo);

			return connectedEvent.Wait(10000);
		}

		public bool JoinChannel(string channel)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			while (!client.IsRegistered && stopwatch.ElapsedMilliseconds < 8000)
				Thread.Sleep(1);

			if (stopwatch.ElapsedMilliseconds >= 8000)
				return false;

			client.Channels.Join(channel);

			return true;
		}

		public void Quit(string message)
		{
			SendChannelMessage(message);
			client.Channels.Leave(Configuration.Channel, "Leaving");
			client.Quit();

			// Give the thread a little bit of time to process the exit
			Thread.Sleep(500);

			Shutdown();
		}

		public void Shutdown()
		{
			// Lock to prevent cross-thread shutdown resource contention
			lock (CommonLock)
			{
				IsRunning = false;
				foreach(Command command in commands)
					command.Shutdown();

				// We do this so it's impossible to accidentally shutdown commands more than once
				commands.Clear();
			}
		}

		private void HandleMessage(object sender, IrcMessageEventArgs e)
		{
			string message = Regex.Replace(e.Text, @"[^\u0020-\u007F]", String.Empty);

			// Direct commands
			if (message.First() == Configuration.CommandDelimiter)
			{
				message = message.Remove(0, 1);
				List<string> tokens = new List<string>(message.Split(new[] { ' ' }));

				Command command = commands.SingleOrDefault(c => c.Prefix == tokens[0]);
				if (command == null)
					return;

				tokens.RemoveAt(0);
				command.HandleDirect(tokens, e.Source.Name);
			}

			// Passive commands
			foreach (Command command in commands)
				command.HandlePassive(message, e.Source.Name);
		}

		public void SendChannelMessage(string value, params object[] args)
		{
			try
			{
				value = Utility.SterilizeString(value);
				client.LocalUser.SendMessage(Configuration.Channel, String.Format(value, args));
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public bool IsUserRegistered(string username)
		{
			bool isRegistered = false;
			bool recieved = false;

			// TODO This subscribes multiple anonymous delegates to the same event
			//      to be honest nothing about this is safe...
			client.LocalUser.NoticeReceived += (sender, args) =>
			{
				isRegistered = args.Text.Split(new[] { ' ' })[2] == "3";
				recieved = true;
			};

			client.SendRawMessage(String.Format("ns status {0}", username));

			Stopwatch timer = new Stopwatch();
			timer.Start();
			while(timer.ElapsedMilliseconds < 150 && !recieved)
				Thread.Sleep(1);

			return isRegistered;
		}
	}
}
