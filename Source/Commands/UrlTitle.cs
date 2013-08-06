using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Assbot.Commands
{
	public class UrlTitle : Command
	{
		public UrlTitle(Bot parent)
			: base(parent)
		{

		}

		public override void Execute(List<string> args)
		{
			if (!new HashSet<string> { "http", "https", "www" }.Any(foo => args[0].StartsWith(foo)))
				return;

			if (args[0].StartsWith("www"))
				args[0] = args[0].Insert(0, "http://");

			Thread thread = new Thread(() =>
			{
				using (WebClient client = new WebClient())
				{
					string page = client.DownloadString(args[0]);
					if (page.Contains("<title>"))
					{
						int titleIndex = page.IndexOf("<title>", StringComparison.Ordinal) + 7;
						int titleEndIndex = page.IndexOf("</title>", StringComparison.Ordinal);

						Parent.SendChannelMessage(page.Substring(titleIndex, titleEndIndex - titleIndex));
					}
				}
			});

			thread.Start();

			base.Execute(args);
		}
	}
}
