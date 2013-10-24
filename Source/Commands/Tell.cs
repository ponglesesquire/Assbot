using System;
using System.Collections.Generic;
using System.Linq;

namespace Assbot.Commands
{
	public class Tell : Command
	{
		public override string Prefix
		{
			get
			{
				return "tell";
			}
		}

		private readonly Dictionary<string, List<TellRecord>> tellRecords;

		public Tell(Bot parent)
			: base(parent)
		{
			tellRecords = new Dictionary<string, List<TellRecord>>();
		}

		public override void HandleDirect(List<string> args, string username)
		{
			if (args.Count < 2)
			{
				Parent.SendChannelMessage("!tell <username> <message>");
				return;
			}

			string toUsername = args.First();
			args.RemoveAt(0);
			string toMessage = String.Join(" ", args.ToArray());

			TellRecord record = new TellRecord(username, toMessage);
			if (tellRecords.ContainsKey(toUsername))
				tellRecords[toUsername].Add(record);
			else
				tellRecords.Add(toUsername, new List<TellRecord> { record });

			Parent.SendChannelMessage("{0} will be told next time they say something.", toUsername);
		}

		public override void HandlePassive(string message, string username)
		{
			if (message.StartsWith("Used left") || !tellRecords.ContainsKey(username))
				return;

			foreach(TellRecord record in tellRecords[username])
				Parent.SendChannelMessage("{0}, {1} had a message for you, \"{2}\".", username, record.Username, record.Message);

			tellRecords[username].Clear();
		}

		private struct TellRecord
		{
			public readonly string Username;
			public readonly string Message;

			public TellRecord(string username, string message)
			{
				Username = username;
				Message = message;
			}
		}
	}
}
