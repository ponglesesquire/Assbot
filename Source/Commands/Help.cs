using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assbot.Commands
{
	public class Help : Command
	{
		public override string Prefix
		{
			get
			{
				return "help";
			}
		}

		public Help(Bot parent)
			: base(parent)
		{

		}

		public override void HandleDirect(List<string> args, string username)
		{
			StringBuilder commands = new StringBuilder("Available commands are: ");

			List<Command> commandList = GetCommands(Parent);
			foreach (Command command in commandList.Where(c => !String.IsNullOrEmpty(c.Prefix)))
				commands.AppendFormat("{0}, ", command.Prefix);

			Parent.SendChannelMessage(commands.ToString(0, commands.Length - 2));
		}
	}
}
