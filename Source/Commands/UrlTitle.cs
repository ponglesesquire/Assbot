using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Assbot.Commands
{
	public class UrlTitle : Command
	{
		private const string UrlRegex = @"(http|ftp|https)://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?";
		private const string TitleRegex = @"<title>\s*(.+?)\s*</title>";

		public UrlTitle(Bot parent)
			: base(parent)
		{
			
		}

		public override void HandlePassive(string message, string username)
		{
			if (!new HashSet<string> { "http", "https", "www" }.Any(message.Contains))
				return;

			if (message.Contains("www.") && !message.Contains("://www."))
				message = message.Replace("www.", "http://www.");

			Match urlMatch = Regex.Match(message, UrlRegex);
			if (!urlMatch.Success)
				return;

			message = urlMatch.Value;

			Thread thread = new Thread(() =>
			{
				string page = Utility.GetHtml(message);
				Match match = Regex.Match(page, TitleRegex);
				if (!match.Success)
					return;

				Parent.SendChannelMessage(match.Groups[1].Value);
			});

			thread.Start();
		}
	}
}
