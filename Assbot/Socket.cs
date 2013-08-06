using System.IO;
using System.Net.Sockets;

namespace Assbot
{
    class Socket
    {
        TcpClient tcpclient;
        StreamReader streamreader;
        StreamWriter streamwriter;
        public bool Connected = false;

        public Socket() { }
        ~Socket() { Dispose(); }

        void Dispose()
        {
            if (tcpclient != null)
                tcpclient.Close();
            if (streamreader != null)
                streamreader.Close();
            if (streamwriter != null)
                streamwriter.Close();
        }

        public bool Connect(string host, int port)
        {
            if (Connected)
                Disconnect();

            try
            {
                tcpclient = new TcpClient(host, port);
                NetworkStream networkstream = tcpclient.GetStream();
                streamreader = new StreamReader(networkstream);
                streamwriter = new StreamWriter(networkstream);
                Connected = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Disconnect()
        {

            Dispose();
            Connected = false;
        }

        public string ReadLine()
        {
            try { return streamreader.ReadLine(); }
            catch { return ""; }
        }
        public void WriteLine(string line)
        {
            try
            {
                streamwriter.WriteLine(line);
                streamwriter.Flush();
            }
            catch { }
        }
    }
}
