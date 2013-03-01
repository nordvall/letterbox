using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Letterbox.WebClient.Web
{
    public interface IWebClient
    {
        HttpWebResponse SendRequest(HttpWebRequest request);
        IAsyncResult BeginSendRequest(HttpWebRequest request, AsyncCallback callback, object state);
        HttpWebResponse EndSendRequest(IAsyncResult asyncResult);
    }
}
