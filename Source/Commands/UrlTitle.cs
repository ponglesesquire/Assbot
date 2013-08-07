using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Assbot.Commands
{
	public class UrlTitle : Command
	{
		public UrlTitle(Bot parent)
			: base(parent)
		{

		}

		public override void HandlePassive(string message, string username)
		{
			if (!new HashSet<string> { "http", "https", "www" }.Any(message.StartsWith))
				return;

			if (message.StartsWith("www"))
				message = message.Insert(0, "http://");

			Thread thread = new Thread(() =>
			{
				using (WebClient client = new WebClient())
				{
					string page = client.DownloadString(message);
					if (!page.Contains("<title>"))
						return;

					int titleIndex = page.IndexOf("<title>", StringComparison.Ordinal) + 7;
					int titleEndIndex = page.IndexOf("</title>", StringComparison.Ordinal);

					Parent.SendChannelMessage(page.Substring(titleIndex, titleEndIndex - titleIndex));
				}
			});

			thread.Start();
		}
	}
}
