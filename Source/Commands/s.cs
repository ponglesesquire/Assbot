using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Assbot.Commands
{
    public class s : Command
    {
        List<string[]> history = new List<string[]>();
        public s(Bot parent) : base(parent)
        {

        }

        public override void HandlePassive(string message, string username)
        {
            history.Add(new string[] { username, message });

            if (history.Count > 5)
                history.RemoveAt(0);

            if (message.StartsWith("s/"))
            {
                string[] temp = message.Split('/');

                if (temp.Length == 3)
                {
                    string result = "";
                    foreach (string[] s in history)
                    {
                        if (Regex.Match(s[1], temp[1]).Success)
                        {
                            result = Regex.Replace(s[1], temp[1], temp[2]);
                            if (result != s[1] || result != String.Empty)
                            {
                                Parent.SendChannelMessage("<" + s[0] + "> " + result);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
