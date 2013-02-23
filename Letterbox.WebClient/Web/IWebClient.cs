using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Letterbox.WebClient.Web
{
    public interface IWebClient
    {
        string SendRequest(HttpWebRequest request);
    }
}
