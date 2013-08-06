using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Assbot
{
	public abstract class Command
	{
		public virtual string Prefix
		{
			get
			{
				return "";
			}
		}

		protected Bot Parent;

		protected Command(Bot parent)
		{
			Parent = parent;
		}

		public virtual void HandleDirect(List<string> args, string username)
		{

		}

		public virtual void HandlePassive(string message, string username)
		{
			
		}

		public static List<Command> GetCommands(Bot parent)
		{
			Assembly assembly = Assembly.GetCallingAssembly();
			IEnumerable<Type> types = assembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(Command)));

			return types.Select(type => (Command)Activator.CreateInstance(type, parent)).ToList();
		}
	}
}
