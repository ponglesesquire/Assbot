using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Assbot.Commands
{
	public class Wiki : Command
	{
		private const string Url = "http://www.wikipedia.org/search-redirect.php?family=wikipedia&search={0}&language=en&go=Go";

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
				try
				{
					string query = Uri.EscapeUriString(String.Join(" ", args));
					string uri = String.Format(Url, query);

					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

					Parent.SendChannelMessage(request.GetResponse().ResponseUri.AbsoluteUri);
				}
				catch (Exception)
				{
					Parent.SendChannelMessage("Something went wrong, I couldn't wiki that for you {0}.", username);
				}

				base.HandleDirect(args, username);
			});

			wikiThread.Start();
		}
	}
}
