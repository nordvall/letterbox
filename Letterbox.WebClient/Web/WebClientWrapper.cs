using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Letterbox.WebClient.Web
{
    public class WebClientWrapper : IWebClient
    {
        public HttpWebResponse SendRequest(HttpWebRequest request)
        {
            return request.GetResponse() as HttpWebResponse;
        }

        public IAsyncResult BeginSendRequest(HttpWebRequest request, AsyncCallback callback, object state)
        {
            return request.BeginGetResponse(callback, state);
        }

        public HttpWebResponse EndSendRequest(IAsyncResult asyncResult)
        {
            var request = asyncResult.AsyncState as HttpWebRequest;
            return request.EndGetResponse(asyncResult) as HttpWebResponse;
        }
    }
}
