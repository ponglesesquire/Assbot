using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Assbot.Commands
{
	public class UrlTitle : Command
	{
		private const string UrlRegex = @"[((http|ftp|https)://|www)]([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?";
		private const string TitleRegex = @"<title>\s*(.+?)\s*</title>";

		public UrlTitle(Bot parent)
			: base(parent)
		{

		}

		public override void HandlePassive(string message, string username)
		{
			if (!new HashSet<string> { "http", "https", "www" }.Any(message.Contains))
				return;

			try
			{
				Match match = Regex.Match(message, UrlRegex);
				if (!match.Success)
					return;

				message = match.Value;
			}
			catch
			{
				return;
			}

			if (message.StartsWith("www"))
				message = message.Insert(0, "http://");

			Thread thread = new Thread(() =>
			{
				using (WebClient client = new WebClient { Encoding = Encoding.UTF8 })
				{
					string page = WebUtility.HtmlDecode(client.DownloadString(message));

					Match match = Regex.Match(page, TitleRegex);
					if (!match.Success)
						return;

					Parent.SendChannelMessage(match.Groups[1].Value);
				}
			});

			thread.Start();
		}
	}
}
