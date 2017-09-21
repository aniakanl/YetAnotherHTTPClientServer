using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YetAnotherWebServer
{
    public class HTTPResponse
    {
        public const string HTTP_VERSION = "HTTP/1.1";
        public HTTPResponseStatus Status { get; set; }
        public byte[] Body { get; set; }

        public byte[] GetResponseStream()
        {
            int bodySize = Body == null ? 0 : Body.Length;

            string header = string.Format("{0} {1} {2}\r\nContent-Length: {3}\r\n\r\n", HTTP_VERSION, (int)Status, GetStatusMessage(Status), bodySize);

            byte[] headerStream = ASCIIEncoding.GetEncoding(0).GetBytes(header);

            byte[] stream;
            if (bodySize>0)
            {
                stream =  new byte[headerStream.Length + Body.Length];
                
                Array.Copy(Body, 0, stream, header.Length, Body.Length);
            }
            else
            {
                stream = new byte[headerStream.Length];
            }

            Array.Copy(headerStream, stream, header.Length);

            return stream;
        }
        public static string GetStatusMessage(HTTPResponseStatus status)
        {
            string result="Unknown";

            switch(status)
            {
                case HTTPResponseStatus.OK:
                    result = "OK";
                    break;

                case HTTPResponseStatus.NotFound:
                    result = "Not Found";
                    break;
                case HTTPResponseStatus.InternalServerError:
                    result = "Internal Server Error";
                    break;
                case HTTPResponseStatus.NotImplemented:
                    result = "Not Implemented";
                    break;
            }
            return result;

        }
    }

    public enum HTTPResponseStatus
    {
        OK = 200,
        NotFound = 404,
        InternalServerError = 500,
        NotImplemented = 501,
        Unknown = 0
    }
}
