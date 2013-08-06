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
		private readonly IrcClient client;
		private IrcUserRegistrationInfo RegistrationInfo
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

		public Bot()
		{
			commands = Command.GetCommands(this);

			client = new IrcClient
			{
				FloodPreventer = new IrcStandardFloodPreventer(4, 2000)
			};

			client.Connected += (sender, args) => Console.WriteLine("Connected!");
			client.Disconnected += (sender, args) =>
			{
				// Reconnect
				Connect();
				JoinChannel();
			};

			client.Registered += (sender, args) =>
			{
				IrcClient localClient = (IrcClient)sender;

				localClient.LocalUser.NoticeReceived += (o, eventArgs) => Console.WriteLine(eventArgs.Text);
				localClient.LocalUser.JoinedChannel += (o, eventArgs) =>
				{
					IrcChannel channel = localClient.Channels.FirstOrDefault();
					if (channel == null)
						return;

					Console.WriteLine("Joined channel!");

					channel.MessageReceived += HandleMessage;
				};

				Console.WriteLine("Registered!");
			};
		}

		public void Run()
		{
			Connect();
			JoinChannel();

			while (client.IsConnected)
				Thread.Sleep(1);
		}

		private void Connect()
		{
			// Connect to channel
			ManualResetEventSlim connectedEvent = new ManualResetEventSlim(false);
			client.Connected += (sender, e) => connectedEvent.Set();
			client.Connect(Configuration.Server, false, RegistrationInfo);

			if (connectedEvent.Wait(10000))
				return;

			throw new Exception("Cannot connect to server!");
		}

		private void JoinChannel()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			while (!client.IsRegistered && stopwatch.ElapsedMilliseconds < 8000)
				Thread.Sleep(1);

			if (stopwatch.ElapsedMilliseconds >= 8000)
				throw new Exception("Didn't register in time");

			client.Channels.Join(Configuration.Channel);
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
			client.LocalUser.SendMessage(Configuration.Channel, String.Format(value, args));
		}
	}
}
