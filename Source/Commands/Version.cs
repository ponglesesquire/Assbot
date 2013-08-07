using System.Collections.Generic;
using System.Reflection;

namespace Assbot.Commands
{
	public class Version : Command
	{
		public override string Prefix
		{
			get
			{
				return "version";
			}
		}

		public Version(Bot parent)
			: base(parent)
		{

		}

		public override void HandleDirect(List<string> args, string username)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Parent.SendChannelMessage("Assbot v{0}", assembly.GetName().Version);

			base.HandleDirect(args, username);
		}
	}
}
