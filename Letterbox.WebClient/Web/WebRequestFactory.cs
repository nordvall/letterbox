using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Letterbox.WebClient.Tokens;

namespace Letterbox.WebClient.Web
{
    public class WebRequestFactory
    {
        private ITokenManager _tokenManager;

        public WebRequestFactory(ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        public HttpWebRequest CreateWebRequest(string httpMethod, Uri requestUri)
        {
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 1;
            request.Method = httpMethod;
            request.ContentType = "application/atom+xml;type=entry;charset=utf-8";
            
            InsertAccessToken(request);
            
            return request;
        }

        public HttpWebRequest CreateWebRequest(string httpMethod, Uri requestUri, byte[] data)
        {
            HttpWebRequest request = CreateWebRequest(httpMethod, requestUri);
            request.ContentLength = data.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            return request;
        }

        public HttpWebRequest CreateTokenWebRequest(string httpMethod, Uri requestUri, byte[] data)
        {
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 1;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            return request;
        }

        private void InsertAccessToken(HttpWebRequest request)
        {
            AccessToken token = _tokenManager.GetAccessToken();
            string tokenHeaderValue = string.Format("WRAP access_token=\"{0}\"", token.TokenValue);
            request.Headers.Add(HttpRequestHeader.Authorization, tokenHeaderValue);
        }
    }
}
