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

        // 'Fixed' function to get links from anywhere in the message
        public override void HandlePassive(string message, string username)
        {
            if (!new HashSet<string> { "http", "https", "www" }.Any(message.Contains))
                return;

            if (message.Contains("www"))
                message = message.Insert(message.IndexOf("www"), "http://");

            Thread thread = new Thread(() =>
            {
                using (WebClient client = new WebClient())
                {
                    string page = client.DownloadString("http" + message.Split(new string[] { "http" }, StringSplitOptions.None)[1].Split(new string[] { "http" }, StringSplitOptions.None)[0]);//message.Substring(message.IndexOf("http"), message.IndexOf(" ") - message.IndexOf("http")));
                    if (!page.Contains("<title>"))
                        return;

                    int titleIndex = page.IndexOf("<title>", StringComparison.Ordinal) + 7;
                    int titleEndIndex = page.IndexOf("</title>", StringComparison.Ordinal);

                    Parent.SendChannelMessage(page.Substring(titleIndex, titleEndIndex - titleIndex));
                }
            });

            thread.Start();
        }

		/*public override void HandlePassive(string message, string username)
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
		}*/
	}
}
