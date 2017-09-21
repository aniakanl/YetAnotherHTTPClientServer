using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using YetAnotherWebServer;

namespace YetAnotherWebClient
{
    class Program
    {

        static int READ_BUFFER_SIZE = 4096;

        public static string RootDirectory { get; set; }

        static byte[] buffer;
        static void Main(string[] args)
        {

            if (args.Length == 4)
            {
                string hostname = args[0];
                int port = int.Parse(args[1]);
                HTTPRequestMethod command = (HTTPRequestMethod)Enum.Parse(typeof(HTTPRequestMethod), args[2].ToUpper());
                string filename = args[3];

                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(hostname).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

                filename = filename.Replace('\\', '/');

                using (Socket client = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {


                    HTTPRequest req = new HTTPRequest();
                    req.Header.ProtocolVersion = "HTTP/1.1";
                    req.Header.ResourceLocation = filename;

                    bool isValid = false;
                    if (command == HTTPRequestMethod.GET)
                    {
                        req.Header.Method = HTTPRequestMethod.GET;
                        isValid = true;
                    }
                    else if (command == HTTPRequestMethod.PUT)
                    {
                        req.Header.Method = HTTPRequestMethod.PUT;

                        if (File.Exists(filename) == true)
                        {
                            using (FileStream fs = new FileStream(filename, FileMode.Open))
                            {
                                using (BinaryReader bw = new BinaryReader(fs))
                                {
                                    req.RawBody = bw.ReadBytes((int)fs.Length);
                                }
                            }
                            isValid = true;
                        }
                    }

                    if (isValid == true)
                    {
                        client.Connect(new IPEndPoint(ipv4Addresses[0], port));
                        client.Send(req.GetResponseStream());
                        Console.WriteLine(ASCIIEncoding.GetEncoding(0).GetString(ReadResponse(client)));
                    }
                }



            }

            else
            {
                Console.WriteLine("YetAnotherWebServer hostname port command filename");
            }
        }

        private static byte[] ReadResponse(Socket client)
        {
            byte[] result = null;
            byte[] tmpBuffer;
            buffer = new byte[READ_BUFFER_SIZE];

            StringBuilder bufferStr = new StringBuilder();

            long readByte = 0;

            bool isFinisihed = false;
            while (isFinisihed == false)
            {
                var readSize = client.Receive(buffer, 0, READ_BUFFER_SIZE, SocketFlags.None);
                if (readSize >= 0)
                {
                    if (result == null)
                    {
                        result = new byte[readSize];
                        Array.Copy(buffer, result, readSize);
                    }
                    else
                    {
                        tmpBuffer = new byte[result.Length + readSize];
                        Array.Copy(result, tmpBuffer, result.Length);
                        Array.Copy(buffer, 0, tmpBuffer, result.Length, readSize);
                        result = tmpBuffer;

                    }

                    readByte += readSize;


                }



                if (readSize != READ_BUFFER_SIZE)
                {

                    isFinisihed = true;
                }
            }

            return result;

        }
    }
}
