using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assbot.Commands
{
	public class Seen : Command
	{
		private const string HistoryFilename = "seen.hst";
		private const uint HistoryMagic = 0xA55B0701;
		private const ushort HistoryVersion = 1;

		public override string Prefix
		{
			get
			{
				return "seen";
			}
		}

		private readonly Dictionary<string, LastSeenRecord> lastSeenRecords; 

		public Seen(Bot parent)
			: base(parent)
		{
			lastSeenRecords = new Dictionary<string, LastSeenRecord>();

			try
			{
				LoadHistory();
			}
			catch(IOException)
			{
				Console.WriteLine("Cannot open history file \"{0}\" for reading.", HistoryFilename);
			}
			catch(Exception)
			{
				Console.WriteLine("Some unknown error occurred.");
			}
		}

		public override void HandleDirect(List<string> args, string username)
		{
			if (args.Count < 1)
			{
				Parent.SendChannelMessage("!seen <username>");
				return;
			}

			string lookingForProper = args.First();
			string lookingFor = lookingForProper.ToLower();

			if (lastSeenRecords.ContainsKey(lookingFor))
			{
				LastSeenRecord lastSeenRecord = lastSeenRecords[lookingFor];
				TimeSpan time = DateTime.Now - lastSeenRecord.Time;

				Parent.SendChannelMessage("{0} last seen saying \"{1}\" {2} ago.",
					lookingForProper, lastSeenRecord.Message, Utility.PrettyTime(time));
			}
			else
				Parent.SendChannelMessage("I've never seen {0} before.", lookingFor);
		}

		public override void HandlePassive(string message, string username)
		{
			LastSeenRecord record = new LastSeenRecord(message, DateTime.Now);
			username = username.ToLower();

			if (lastSeenRecords.ContainsKey(username))
				lastSeenRecords[username] = record;
			else
				lastSeenRecords.Add(username, record);
		}

		public override void Shutdown()
		{
			try
			{
				SaveHistory();
			}
			catch (IOException)
			{
				Console.WriteLine("Cannot open history file \"{0}\" for writing.", HistoryFilename);
			}
			catch (Exception)
			{
				Console.WriteLine("Some unknown error occurred.");
			}
		}

		private void LoadHistory()
		{
			Console.Write("Loading seen history... ");
			using (BinaryReader reader = new BinaryReader(new FileStream(HistoryFilename, FileMode.Open)))
			{
				if (reader.ReadUInt32() != HistoryMagic)
				{
					Console.WriteLine();
					Console.WriteLine("Couldn't load seen history, this session will start with a fresh seen history.");
					return;
				}

				if (reader.ReadUInt16() != HistoryVersion)
				{
					Console.WriteLine();
					Console.WriteLine("Seen history file is outdated, this session will start with a fresh seen history.");
					return;
				}

				int count = reader.ReadInt32();
				for(int i = 0; i < count; ++i)
				{
					string username = reader.ReadString();
					string message = reader.ReadString();
					DateTime dateTime = DateTime.FromBinary(reader.ReadInt64());

					lastSeenRecords.Add(username, new LastSeenRecord(message, dateTime));
				}

				Console.WriteLine("Success! Loaded {0} records.", count);
			}
		}

		private void SaveHistory()
		{
			Console.Write("Saving history... ");

			using(BinaryWriter writer = new BinaryWriter(new FileStream(HistoryFilename, FileMode.Create)))
			{
				writer.Write(HistoryMagic);
				writer.Write(HistoryVersion);

				writer.Write(lastSeenRecords.Count);
				foreach(var record in lastSeenRecords)
				{
					writer.Write(record.Key);
					writer.Write(record.Value.Message);
					writer.Write(record.Value.Time.ToBinary());
				}
			}

			Console.WriteLine("Done!");
		}

		private struct LastSeenRecord
		{
			public readonly string Message;
			public readonly DateTime Time;

			public LastSeenRecord(string message, DateTime time)
			{
				Message = message;
				Time = time;
			}
		}
	}
}
