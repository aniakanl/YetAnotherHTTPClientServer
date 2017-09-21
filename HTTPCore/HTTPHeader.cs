using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YetAnotherWebServer
{
    public class HTTPHeader
    {
        public HTTPRequestMethod Method { get; set; }
        public string ResourceLocation { get; set; }
        public string ProtocolVersion { get; set; }

        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        public static HTTPHeader Parse(byte[] rawHeader)
        {
            HTTPHeader header = null;
            string headerStr = ASCIIEncoding.GetEncoding(0).GetString(rawHeader);

            string[] strSplitors = new string[] { "\r\n" };
            string[] lines = headerStr.Split(strSplitors, StringSplitOptions.RemoveEmptyEntries);
            if(lines.Length > 0)
            {
                
                char[] charSplitors = new char[] { ' ' };
                string[] commandLineItems = lines[0].Split(charSplitors, StringSplitOptions.RemoveEmptyEntries);
                if(commandLineItems.Length== 3)
                {
                    header = new HTTPHeader();
                    header.Method = (HTTPRequestMethod)Enum.Parse(typeof (HTTPRequestMethod), commandLineItems[0]);
                    header.ResourceLocation = commandLineItems[1];
                    header.ProtocolVersion = commandLineItems[2];

                    for(int i=1; i<lines.Length; i++)
                    {
                        int index = lines[i].IndexOf(':');
                        if(index >1)
                        {
                            if(index + 1 < lines[i].Length)
                            {
                                header.Headers.Add(lines[i].Substring(0, index ), lines[i].Substring(index + 1));
                            }
                            else
                            {
                                header = null;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                    }

                }

                
            }

            return header;



        }
    }

    public enum HTTPRequestMethod
    {
        GET = 1,
        PUT = 2,
        POST = 3,
        HEAD = 4,
        UNKNOWN = 0
    }
}
