using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assbot.Commands
{
	public class Tell : Command
	{
		private const string HistoryFilename = "tell.hst";
		private const uint HistoryMagic = 0xA55B0702;
		private const ushort HistoryVersion = 1;

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

			try
			{
				LoadHistory();
			}
			catch (IOException)
			{
				Console.WriteLine("Cannot open history file \"{0}\" for reading.", HistoryFilename);
			}
			catch (Exception)
			{
				Console.WriteLine("Some unknown error occurred.");
			}
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
			Console.Write("Loading tell history... ");
			using(BinaryReader reader = new BinaryReader(new FileStream(HistoryFilename, FileMode.Open)))
			{
				if (reader.ReadUInt32() != HistoryMagic)
				{
					Console.WriteLine();
					Console.WriteLine("Couldn't load tell history, this session will start with a fresh tell history.");
					return;
				}

				if (reader.ReadUInt16() != HistoryVersion)
				{
					Console.WriteLine();
					Console.WriteLine("Tell history file is outdated, this session will start with a fresh tell history.");
					return;
				}

				int count = reader.ReadInt32();
				for(int i = 0; i < count; ++i)
				{
					List<TellRecord> record = new List<TellRecord>();
					string toUsername = reader.ReadString();

					int subRecordCount = reader.ReadInt32();
					for(int j = 0; j < subRecordCount; ++j)
					{
						string fromUsername = reader.ReadString();
						string message = reader.ReadString();

						record.Add(new TellRecord(fromUsername, message));
					}

					tellRecords.Add(toUsername, record);
				}

				Console.WriteLine("Success! Loaded {0} records.", count);
			}
		}

		private void SaveHistory()
		{
			Console.Write("Saving tell history... ");
			using(BinaryWriter writer = new BinaryWriter(new FileStream(HistoryFilename, FileMode.Create)))
			{
				writer.Write(HistoryMagic);
				writer.Write(HistoryVersion);

				writer.Write(tellRecords.Count);
				foreach(var record in tellRecords)
				{
					writer.Write(record.Key);
					
					writer.Write(record.Value.Count);
					foreach(var subRecord in record.Value)
					{
						writer.Write(subRecord.Username);
						writer.Write(subRecord.Message);
					}
				}
			}

			Console.WriteLine("Done!");
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
