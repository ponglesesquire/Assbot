using System.Collections.Generic;

namespace Assbot.Commands
{
	public class Quit : Command
	{
		public override string Prefix
		{
			get
			{
				return "quit";
			}
		}

		private static readonly HashSet<string> Operators = new HashSet<string>
		{
			"Rawrity",
			"DatZach"
		};

		public Quit(Bot parent)
			: base(parent)
		{

		}

		public override void HandleDirect(List<string> args, string username)
		{
			if (!Operators.Contains(username))
			{
				Parent.SendChannelMessage("YOU'RE NOT THE BOSS OF ME! >:C");
				return;
			}

			Parent.Quit("I'm out.");

			base.HandleDirect(args, username);
		}
	}
}
