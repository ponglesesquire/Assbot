using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Assbot.Commands
{
	public class Lucky : Command
	{
		public override string Prefix
		{
			get
			{
				return "lucky";
			}
		}

		public Lucky(Bot parent)
			: base(parent)
		{
			// Restaging the cheap way
		}

		public override void HandleDirect(List<string> args, string username)
		{
			if (args.Count < 1)
			{
				Parent.SendChannelMessage("!lucky <query>");
				return;
			}

			Thread thread = new Thread(() =>
			{
				try
				{
					string query = Uri.EscapeUriString(String.Join(" ", args));
					string uri = String.Format("http://www.google.com/search?q={0}&btnI", query);

					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
					Parent.SendChannelMessage(request.GetResponse().ResponseUri.AbsoluteUri);
				}
				catch
				{
					Parent.SendChannelMessage("Something went wrong, sorry {0}.", username);
				}
			});

			thread.Start();
		}
	}
}
