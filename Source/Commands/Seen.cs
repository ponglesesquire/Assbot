using System;
using System.Collections.Generic;
using System.Linq;

namespace Assbot.Commands
{
	public class Seen : Command
	{
		public override string Prefix
		{
			get
			{
				return "seen";
			}
		}

		private readonly Dictionary<string, LastSeenRecord> lastSeen; 

		public Seen(Bot parent)
			: base(parent)
		{
			lastSeen = new Dictionary<string, LastSeenRecord>();
		}

		public override void HandleDirect(List<string> args, string username)
		{
			string lookingFor = args.First();

			if (lastSeen.ContainsKey(lookingFor))
			{
				LastSeenRecord lastSeenRecord = lastSeen[lookingFor];
				Parent.SendChannelMessage("{0} last seen saying \"{1}\" at {2}.", lookingFor, lastSeenRecord.Message, lastSeenRecord.Time.ToString("MMM d hh:mm tt"));
			}
			else
				Parent.SendChannelMessage("I've never seen {0} before.", lookingFor);

			base.HandleDirect(args, username);
		}

		public override void HandlePassive(string message, string username)
		{
			LastSeenRecord record = new LastSeenRecord(message, DateTime.Now);

			if (lastSeen.ContainsKey(username))
				lastSeen[username] = record;
			else
				lastSeen.Add(username, record);

			base.HandlePassive(message, username);
		}

		public struct LastSeenRecord
		{
			public string Message;
			public DateTime Time;

			public LastSeenRecord(string message, DateTime time)
			{
				Message = message;
				Time = time;
			}
		}
	}
}
