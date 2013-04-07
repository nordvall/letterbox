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
            HttpWebResponse response = null;

            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;

                if (response == null)
                {
                    throw;
                }
            }

            return response;
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
