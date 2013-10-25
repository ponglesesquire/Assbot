using System;
using System.Net;
using System.Text;

namespace Assbot
{
	public static class Utility
	{
		public static string GetHtml(string address)
		{
			string contents;

			try
			{
				using (WebClient client = new WebClient { Encoding = Encoding.UTF8 })
					contents = WebUtility.HtmlDecode(client.DownloadString(address));
			}
			catch (Exception)
			{
				return "";
			}

			return contents;
		}
	}
}
