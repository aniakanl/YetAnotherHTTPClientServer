using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

namespace YetAnotherWebServer
{
    class YetAnotherWebServer
    {
        string IP { get; set; }
        int Port { get; set; }
        public YetAnotherWebServer(string ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        public void Run()
        {

            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                server.Bind(new IPEndPoint(IPAddress.Parse(IP), Port));
              
                server.Listen(100);
                WebHandler clientHandler;
                WebHandler.RootDirectory = "http_doc";
                while(true)
                {

                    clientHandler = new WebHandler();
                    clientHandler.Client = server.Accept();
                    

                    ThreadStart ts = new ThreadStart(clientHandler.Process);
                    Thread t = new Thread(ts);
                    t.Start();
                }
                
            }

        }
    }
}
