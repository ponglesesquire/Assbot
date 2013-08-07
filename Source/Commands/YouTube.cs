using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Assbot.Commands
{
	public class YouTube : Command
	{
		public override string Prefix
		{
			get
			{
				return "yt";
			}
		}

		public YouTube(Bot parent)
			: base(parent)
		{

		}

		public override void HandleDirect(List<string> args, string username)
		{
			if (args.Count < 1)
			{
				Parent.SendChannelMessage("!yt <query>");
				return;
			}

			Thread ytThread = new Thread(
			() =>
			{
				string query = Uri.EscapeUriString(String.Join(" ", args));
				string uri = String.Format("http://www.youtube.com/results?search_query={0}", query);

				WebClient client = new WebClient();
				string body = client.DownloadString(uri);
				string results = body.Split(new[] { "<ol id=\"search-results\" class=\"result-list context-data-container\">" },
											StringSplitOptions.None)[1];
				string id = results.Split(new[] { "data-context-item-id=\"" }, StringSplitOptions.None)[1]
								   .Split(new[] { "\"" }, StringSplitOptions.None)[0];
				string title = results.Split(new[] { "data-context-item-title=\"" }, StringSplitOptions.None)[1]
									  .Split(new[] { "\"" },StringSplitOptions.None)[0];

				Parent.SendChannelMessage("{0} - http://www.youtube.com/watch?v={1}", title, id);
			});

			ytThread.Start();

			base.HandleDirect(args, username);
		}
	}
}
