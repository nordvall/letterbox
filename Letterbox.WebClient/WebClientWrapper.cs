using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Letterbox.WebClient
{
    public class WebClientWrapper : IWebClient
    {
        public string SendRequest(HttpWebRequest request)
        {
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string content = reader.ReadToEnd();
                        return content;
                    }
                }
            }
        }
    }
}
