using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YetAnotherWebServer
{
    public class HTTPRequest
    {
        public const string HTTP_VERSION = "HTTP/1.1";
        public HTTPHeader Header { get; set; }

        public byte[] RawBody { get; set; }

        public int RawHeaderSize { get; set; }
        public HTTPRequest()
        {
            Header = new HTTPHeader();
        }

        public static HTTPRequest Parse(byte[] rawReq)
        {
            HTTPRequest result = null;
            byte[] headerRaw = ExtractHeader(rawReq);

            HTTPHeader header = HTTPHeader.Parse(headerRaw);
            if(header != null)
            {
                result = new HTTPRequest();
                result.RawBody = new byte[rawReq.Length - headerRaw.Length];
                Array.Copy(rawReq, headerRaw.Length, result.RawBody, 0, rawReq.Length - headerRaw.Length);
                result.RawHeaderSize = headerRaw.Length;
                result.Header = header;
            }

            return result;
        }

        private static byte[] ExtractHeader(byte[] rawReq)
        {
            byte[] result = null;
            int state = 0;
            if(rawReq.Length >1)
            {
                int endOfHeaderLoc = -1;
                for(int index = 1;  index < rawReq.Length; index++)
                {
                    // Simple dfa to detect \r\n\r\n
                    if( state ==0  )
                    {
                        state = (rawReq[index] == '\r') ? 1 : 0;
                    }
                    else if(state ==1)
                    {
                        state = (rawReq[index] == '\n') ? 2 : 0;
                    }
                    else if(state == 2)
                    {
                        state = (rawReq[index] == '\r') ? 3 : 0;
                    }
                    else if(state == 3)
                    {
                        if(rawReq[index]=='\n')
                        {
                            endOfHeaderLoc = index;
                           
                            break;
                        }
                        else
                        {
                            state = 0;
                        }
                    }
                }

                if(endOfHeaderLoc >0)
                {
                    result = new byte[endOfHeaderLoc+1];
                    Array.Copy(rawReq, result, endOfHeaderLoc + 1);
                }
            }

            return result;
        }

        public byte[] GetResponseStream()
        {
            int bodySize = RawBody == null ? 0 : RawBody.Length;

            string header = string.Format("{0} {1} {2}\r\nContent-Length: {3}\r\n\r\n", Header.Method, Header.ResourceLocation, Header.ProtocolVersion, bodySize);

            byte[] headerStream = ASCIIEncoding.GetEncoding(0).GetBytes(header);

            byte[] stream;
            if (bodySize > 0)
            {
                stream = new byte[headerStream.Length + RawBody.Length];

                Array.Copy(RawBody, 0, stream, header.Length, RawBody.Length);
            }
            else
            {
                stream = new byte[headerStream.Length];
            }

            Array.Copy(headerStream, stream, header.Length);

            return stream;
        }
    }
}
