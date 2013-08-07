﻿using System;
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
			if (args.Count < 1)
			{
				Parent.SendChannelMessage("!seen <username>");
				return;
			}

			string lookingFor = args.First().ToLower();

			if (lastSeen.ContainsKey(lookingFor))
			{
				LastSeenRecord lastSeenRecord = lastSeen[lookingFor];
				string lastTime = lastSeenRecord.Time.ToString("MMM d hh:mm tt");

				Parent.SendChannelMessage("{0} last seen saying \"{1}\" at {2}.",
					lastSeenRecord.Username, lastSeenRecord.Message, lastTime);
			}
			else
				Parent.SendChannelMessage("I've never seen {0} before.", lookingFor);

			base.HandleDirect(args, username);
		}

		public override void HandlePassive(string message, string username)
		{
			LastSeenRecord record = new LastSeenRecord(username, message, DateTime.Now);
			username = username.ToLower();

			if (lastSeen.ContainsKey(username))
				lastSeen[username] = record;
			else
				lastSeen.Add(username, record);

			base.HandlePassive(message, username);
		}

		private struct LastSeenRecord
		{
			public readonly string Username;
			public readonly string Message;
			public DateTime Time;

			public LastSeenRecord(string username, string message, DateTime time)
			{
				Username = username;
				Message = message;
				Time = time;
			}
		}
	}
}
