using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Assbot.Commands
{
	public class Google : Command
	{
		public override string Prefix
		{
			get
			{
				return "google";
			}
		}

		public Google(Bot parent)
			: base(parent)
		{

		}

		public override void HandleDirect(List<string> args, string username)
		{
			if (args.Count < 1)
			{
				Parent.SendChannelMessage("!{0} <query>", Prefix);
				return;
			}

			Thread thread = new Thread(() =>
			{
				string query = Uri.EscapeUriString(String.Join(" ", args));
				string uri = String.Format("http://www.google.com/search?q={0}&btnI", query);

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

				Parent.SendChannelMessage(request.GetResponse().ResponseUri.AbsoluteUri);
			});

			thread.Start();

			base.HandleDirect(args, username);
		}
	}
}
