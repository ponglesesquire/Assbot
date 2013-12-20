using System;
using System.Net;
using System.Text;

namespace Assbot
{
	public static class Utility
	{
		public static string SterilizeString(string value)
		{
			for (int i = 0; i < value.Length; ++i)
			{
				switch (value[i])
				{
					case '{':
					{
						int tokenPosition = i++;
						for (; i < value.Length; ++i)
						{
							if (value[i] == '}')
							{
								tokenPosition = -1;
								break;
							}

							if (!char.IsDigit(value[i]))
								break;
						}

						if (tokenPosition != -1)
							value = value.Insert(i++, "{");

						break;
					}

					case '}':
						value = value.Insert(i++, "}");
						break;
				}
			}

			return value;
		}

		public static string PrettyTime(TimeSpan timeSpan)
		{
			StringBuilder builder = new StringBuilder();

			if (timeSpan.Days > 0)
				builder.AppendFormat("{0} {1}, ", timeSpan.Days, timeSpan.Days == 1 ? "day" : "days");

			if (timeSpan.Hours > 0)
				builder.AppendFormat("{0} {1}, ", timeSpan.Hours, timeSpan.Days == 1 ? "hour" : "hours");

			if (timeSpan.Minutes > 0)
				builder.AppendFormat("{0} {1}", timeSpan.Minutes, timeSpan.Minutes == 1 ? "minute" : "minutes");
            else
			    builder.AppendFormat("{0} {1}", timeSpan.Seconds, timeSpan.Seconds == 1 ? "second" : "seconds");

			return builder.ToString();
		}

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
