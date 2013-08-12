using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Assbot.Commands
{
	public class Wiki : Command
	{
		public override string Prefix
		{
			get
			{
				return "wiki";
			}
		}

		public Wiki(Bot parent)
			: base(parent)
		{

		}

		public override void HandleDirect(List<string> args, string username)
		{
			Thread wikiThread = new Thread(() =>
			{
				string query = Uri.EscapeUriString(String.Join(" ", args));
				string uri =
					String.Format("http://www.wikipedia.org/search-redirect.php?family=wikipedia&search={0}&language=en&go=Go", query);

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

				Parent.SendChannelMessage(request.GetResponse().ResponseUri.AbsoluteUri);
				base.HandleDirect(args, username);
			});

			wikiThread.Start();
		}
	}
}
