using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using System.Net;
using System.IO;


namespace Assbot
{
    class Bot
    {
        Dictionary<string, List<string>> tells = new Dictionary<string, List<string>>();

        List<Plugin> plugins = new List<Plugin>();
        Socket socket;
        bool C = true;

        public Bot()
        {
            Output("Assbot v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            
            Connect("irc.twigathy.com");
        }
        ~Bot() { Dispose(); }

        public void Dispose()
        {
            C = false;
            Disconnect();
            socket.Connected = false;
        }

        #region (Dis)Connect
        public void Connect(string host, int port = 6667)
        {
            socket = new Socket();
            socket.Connect(host, port);

            Thread thread = new Thread(new ParameterizedThreadStart(Read));
            thread.Start(socket);
        }
        public void Disconnect()
        {
            if (socket.Connected)
                socket.Disconnect();
        }
        #endregion

        void Read(object s)
        {
            socket = (Socket)s;

            socket.WriteLine("NICK Assbot1");
            socket.WriteLine("USER user 0 * :bot");

            string data = "";
            System.Net.WebClient webclient = new System.Net.WebClient();
            StreamWriter writer = new StreamWriter("LOG", true);

            #region while
            while (C)
            {
                string[] part = (data = socket.ReadLine()).Split();
                if (data == "")
                    break;

                if (part[0] == "PING")
                {
                    socket.WriteLine("PONG " + part[1]);
                    Console.WriteLine("Ping - Pong!");
                }
                else
                {
                    if (part.Length >= 2)
                    {
                        if (new string[] { "JOIN", "PART", "QUIT", "PRIVMSG", "NICK" }.Contains(part[1]))
                        {
                            //writer.WriteLine(data.Split(':')[1].Split('!')[0] + " " + part[1] + " " + part[2] + " " + );
                            writer.WriteLine(data);
                            writer.Flush();
                        }
                    }

                    Output(data);

                    if (part.Length > 1)
                    {
                        switch (part[1])
                        {
                            case "001":
                                socket.WriteLine("JOIN #bots");
                                break;
                        }
                    }

                    if ((data.Contains("http://") || data.Contains("https://")) && part[1] == "PRIVMSG" && !part[0].Contains("Botify"))
                    {
                        try
                        {
                            string link = data.Split(new string[] { "://" }, StringSplitOptions.None)[1].Split(' ')[0];
                            socket.WriteLine("PRIVMSG " + part[2] + " :" + WebUtility.HtmlDecode(webclient.DownloadString("http://" + link).Split(new string[] { "<title>" }, StringSplitOptions.None)[1].Split(new string[] { @"</title>" }, StringSplitOptions.None)[0]));
                        }
                        catch { }
                    }





                    if (part.Length > 3)
                    {
                        if (part[1] == "PRIVMSG")
                        {
                            if (part[3] == ":@yt")
                            {
                                string search = getparts(part, 4);
                                string body = webclient.DownloadString("http://www.youtube.com/results?search_query=" + search.Replace(' ', '+'));
                                string results = body.Split(new string[] { "<ol id=\"search-results\" class=\"result-list context-data-container\">" }, StringSplitOptions.None)[1];
                                string id = results.Split(new string[] { "data-context-item-id=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0];
                                string title = results.Split(new string[] { "data-context-item-title=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0];
                                socket.WriteLine("PRIVMSG " + part[2] + " :" + title + " - " + "http://youtube.com/watch?v=" + id);
                            }

                            if (part[3] == ":@g")
                            {
                                string search = getparts(part, 4);
                                string body = webclient.DownloadString("http://www.google.com/search?q=" + search.Replace(' ', '+') + "&btnI");
                                string result = body.Split(new string[] { "<a href=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0];
                                socket.WriteLine("PRIVMSG " + part[2] + " :" + result);
                            }

                            if (part[3] == ":@gmlwiki")
                            {
                                string url = "http://wiki.yoyogames.com/index.php/" + part[4];
                                string page = "";

                                try { page = webclient.DownloadString(url); }
                                catch(WebException e) { socket.WriteLine("PRIVMSG " + part[2] + " :" + e); }

                                if (page != "")
                                {
                                    string desc = page.Split(new string[] { "<p>" }, StringSplitOptions.None)[1].Split(new string[] { "</p>" }, StringSplitOptions.None)[0];
                                    socket.WriteLine("PRIVMSG " + part[2] + " :" + url + " - " + desc);
                                }
                            }
                        }
                    }





                    




                    if (part.Length > 1)
                    {
                        if (part[1] == "JOIN")
                        {
                            string name = part[0].Substring(1, part[0].IndexOf('!')-1);
                            if (tells.ContainsKey(name))
                                foreach (string msg in tells[name])
                                {
                                    socket.WriteLine("PRIVMSG " + part[2].Substring(1) + " :" + name + ": " + msg);
                                    //tells[name].Remove(msg);
                                }
                        }

                        if (part[1] == "PRIVMSG")
                        {
                            if (part[3].ToLower() == ":@seen")
                            {
                                if (part[0].ToLower().Contains(part[4].ToLower()))
                                    socket.WriteLine("PRIVMSG " + part[2] + " :no");
                                else
                                {
                                    writer.Dispose();
                                    var file = File.ReadAllLines("LOG").Reverse();
                                    writer = new StreamWriter("LOG", true);
                                    foreach (string line in file)
                                    {
                                        if (line.ToLower().Split(':')[1].Contains(part[4].ToLower()))
                                        {
                                            socket.WriteLine("PRIVMSG " + part[2] + " :\'" + part[4] + "\' was last seen saying \"" + line.Replace(line.Split(':')[1], "").Remove(0, 2) + "\"");
                                            break;
                                        }
                                    }
                                }
                            }

                            // Tell
                            if (part[3].ToLower() == ":@tell")
                            {
                                string str = getparts(part, 5);
                                if (!tells.ContainsKey(part[4]))
                                    tells.Add(part[4], new List<string>() { str });
                                else
                                    tells[part[4]].Add(str);

                                socket.WriteLine("PRIVMSG " + part[2] + " :" + part[4] + " will be told \"" + str + "\" next time they join.");
                            }
                        }
                    }






                    /*if (part[1] == "PRIVMSG")
                    {
                        // Seen
                        if (part[3].ToLower() == ":@seen")
                        {
                            writer.Dispose();

                            ReverseLineReader reader = new ReverseLineReader("LOG");
                            reader.ToString().
                            //socket.WriteLine("PRIVMSG " + part[2] + " :" + reader.ReadLine());

                            writer = new StreamWriter("LOG", true);
                        }
                    }*/






                    // !Python
                    /*
                    if (part[1] == "PRIVMSG")
                    {
                        if (part[3] == ":!py")
                        {
                            var proc = new System.Diagnostics.Process
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "E:\\Python27\\python.exe",
                                    UseShellExecute = false,
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true
                                }
                            };
                            proc.Start();
                            proc.StandardInput.WriteLine("1+1");
                            string line = "null";
                            line = proc.StandardOutput.ReadLine();
                            Output("Python: " + line);
                            Send("PRIVMSG " + part[2] + " :" + line);
                            proc.Close();
                        }
                    }*/


                    /*
                    switch (part.Length)
                    {
                        case 0: Output("0"); break;
                        case 1: Output("1"); break;
                        case 2: Output("2"); break;
                        case 3: Output("3"); break;
                        case 4: Output("4"); break;
                        case 5: Output("5"); break;
                        default: Output(data); break;
                    }
                    */

                }
            }
            #endregion

            writer.Dispose();
            webclient.Dispose();
        }

        #region Plugins
        void LoadPlugins()
        {
            object[] o = { this };
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            foreach (string dll in System.IO.Directory.GetFiles(path + "\\Plugins\\", "*.dll"))
            {
                {
                    Console.WriteLine("Loading Assembly \"" + dll + "\"");
                    System.Reflection.Assembly a = System.Reflection.Assembly.LoadFile(dll);
                    foreach (Type t in a.GetTypes())
                        if (t.BaseType == typeof(Plugin))
                        {
                            Plugin p = (Plugin)Activator.CreateInstance(t, o);
                            plugins.Add(p);
                        }
                }
            }
        }
        #endregion
        #region Misc
        public void Send(string str) { socket.WriteLine(str); }

        void Output(string str) { Console.WriteLine(str); }
        void Output(string str, ConsoleColor color) { Console.ForegroundColor = color; Console.WriteLine(str); Console.ResetColor(); }
        #endregion

        string getparts(string[] parts, int start)
        {
            string temp = "";
            for (int i = start; i < parts.Length-1; ++i)
                temp += parts[i] + " ";
            return temp + parts[parts.Length-1];
        }
    }
}
