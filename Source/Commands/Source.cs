using System.Collections.Generic;

namespace Assbot.Commands
{
	public class Source : Command
	{
		public override string Prefix
		{
			get
			{
				return "source";
			}
		}

		public Source(Bot parent)
			: base(parent)
		{

		}

		public override void HandleDirect(List<string> args, string username)
		{
			Parent.SendChannelMessage("https://github.com/Rixlet/Assbot");
		}
	}
}
