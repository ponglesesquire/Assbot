using System;
using System.Collections.Generic;

namespace Assbot.Commands
{
	public class UpTime : Command
	{
		public override string Prefix
		{
			get
			{
				return "uptime";
			}
		}

		private readonly DateTime startTime;

		public UpTime(Bot parent)
			: base(parent)
		{
			startTime = DateTime.Now;
		}

		public override void HandleDirect(List<string> args, string username)
		{
			TimeSpan upTime = DateTime.Now - startTime;
			Parent.SendChannelMessage("Assbot has been up for {0}.", Utility.PrettyTime(upTime));
		}
	}
}
