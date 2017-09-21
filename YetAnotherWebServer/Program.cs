using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YetAnotherWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                YetAnotherWebServer server = new YetAnotherWebServer("0.0.0.0", int.Parse(args[0]));
                server.Run();
            }
            else
            {
                Console.WriteLine("YetAnotherWebServer port");
            }
        }


    }
}
