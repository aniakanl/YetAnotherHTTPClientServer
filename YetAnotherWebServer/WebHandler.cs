using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YetAnotherWebServer
{
    class WebHandler
    {
        static int READ_BUFFER_SIZE = 4096;

        public static string RootDirectory { get; set; } 

        byte[] buffer;

        public Socket Client { get; set; }
        public void Process()
        {

            try
            {
                byte[] rawRequest = ReadRequest();


                HTTPRequest request = HTTPRequest.Parse(rawRequest);

                if (request != null)
                {
                    if (request.Header.Method == HTTPRequestMethod.GET)
                    {
                        HandleGetMethod(request);

                    }
                    else if (request.Header.Method == HTTPRequestMethod.PUT)
                    {
                        HandlePutMethod(request);

                    }
                    else
                    {
                        HTTPResponse response = new HTTPResponse();
                        response.Status = HTTPResponseStatus.NotImplemented;
                        Client.Send(response.GetResponseStream());
                    }
                }

                Console.WriteLine(ASCIIEncoding.GetEncoding(0).GetString(rawRequest));
            }
            catch(Exception exp)
            {
                HTTPResponse response = new HTTPResponse();
                response.Status = HTTPResponseStatus.InternalServerError;
                Client.Send(response.GetResponseStream());
            }

            Client.Close();
        }

        private void HandlePutMethod(HTTPRequest request)
        {

                string path = GetLocalPath(request.Header.ResourceLocation);
                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(request.RawBody);
                    }
                }

                HTTPResponse response = new HTTPResponse();
                response.Status = HTTPResponseStatus.OK;
                Client.Send(response.GetResponseStream());

        }


        private void HandleGetMethod(HTTPRequest request)
        {

            string path = GetLocalPath(request.Header.ResourceLocation);

            if (File.Exists(path))
            {
                byte[] data = null;
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {

                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        if(fs.Length>0 && fs.Length<=int.MaxValue)
                        {
                            data = br.ReadBytes((int)fs.Length);
                        }
                            
                    }
                }

                HTTPResponse response = new HTTPResponse();
                response.Body = data;
                response.Status = HTTPResponseStatus.OK;
                Client.Send(response.GetResponseStream());


            }
            else
            {
                HTTPResponse response = new HTTPResponse();
                response.Status = HTTPResponseStatus.NotFound;
                Client.Send(response.GetResponseStream());
            }
        }

        private string GetLocalPath(string resourcePath)
        {

            string localPath = resourcePath.Replace('/', '\\');


            if (localPath.EndsWith("\\"))
            {
                localPath = localPath + "index.html";
            }



            // removing the leading slash. Slash is root directory
            if (localPath.StartsWith("\\"))
            {
                localPath = localPath.Substring(1);
            }


            string absPath = Path.Combine(RootDirectory, localPath);

            return absPath;
        }

        private byte[] ReadRequest()
        {
            byte[] result = null;
            byte[] tmpBuffer;
            buffer = new byte[READ_BUFFER_SIZE];

            StringBuilder bufferStr = new StringBuilder();

            long readByte = 0;

            bool isFinisihed = false;
            while(isFinisihed == false)
            {
                var readSize = Client.Receive(buffer, 0, READ_BUFFER_SIZE, SocketFlags.None);
                if (readSize >= 0)
                {
                    if(result == null)
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


                
                if(readSize != READ_BUFFER_SIZE)
                {

                    isFinisihed = true;
                }
            }

            return result;

        }
    }
}
