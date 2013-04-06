using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Letterbox.Clients;
using Letterbox.WebClient.Web;

namespace Letterbox.WebClient.Tokens
{
    public class WebTokenProvider : IWebTokenProvider
    {
        private IWebClient _webClient;
        private Uri _stsAddress;
        private WebToken _cachedToken;
        private NetworkCredential _credential;

        public WebTokenProvider(Uri stsUri)
            : this(stsUri, null, new WebClientWrapper())
        { }

        public WebTokenProvider(Uri stsUri, NetworkCredential credential)
            : this(stsUri, credential, new WebClientWrapper())
        { }

        public WebTokenProvider(Uri stsUri, NetworkCredential credential, IWebClient webClient)
        {
            _webClient = webClient;
            _stsAddress = stsUri;
            _credential = credential;
        }

        public WebToken GetAccessToken()
        {
            if (_cachedToken == null || _cachedToken.Exprires < DateTime.UtcNow.AddMinutes(5))
            {
                if (_credential == null)
                {
                    _cachedToken = RequestWindowsToken();
                }
                else
                {
                    _cachedToken = RequestOAuthToken(_credential.UserName, _credential.Password);
                }
            }

            return _cachedToken;
        }
        
        private WebToken RequestOAuthToken(string userName, string userPassword)
        {
            const string ClientPasswordFormat =
                "grant_type=authorization_code&client_id={0}&client_secret={1}&scope={2}";

            Uri requestUri = GenerateTokenRequestUri("OAuth");
            string requestContent = string.Format(CultureInfo.InvariantCulture,
                ClientPasswordFormat, HttpUtility.UrlEncode(userName),
                HttpUtility.UrlEncode(userPassword),
                HttpUtility.UrlEncode(_stsAddress.AbsoluteUri));
            byte[] data = Encoding.UTF8.GetBytes(requestContent);

            WebToken token = RequestToken(requestUri, data);
            return token;
        }

        private WebToken RequestWindowsToken()
        {
            Uri requestUri = GenerateTokenRequestUri("Windows");
            string requestContent = string.Format("scope={0}", HttpUtility.UrlEncode(_stsAddress.AbsoluteUri));
            byte[] data = Encoding.UTF8.GetBytes(requestContent);

            WebToken token = RequestToken(requestUri, data);
            return token;
        }

        private WebToken RequestToken(Uri requestUri, byte[] data)
        {
            var requestFactory = new WebRequestFactory(this);
            HttpWebRequest request = requestFactory.CreateTokenWebRequest("POST", requestUri, data);

            HttpWebResponse response = _webClient.SendRequest(request);
            using (Stream stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string content = reader.ReadToEnd();
                    var accessToken = new WebToken(content);
                    return accessToken;
                }
            }
        }


        private Uri GenerateTokenRequestUri(string authMethod)
        {
            string address = string.Format("{0}://{1}:{2}/{3}/$STS/{4}/", _stsAddress.Scheme, _stsAddress.DnsSafeHost, _stsAddress.Port, _stsAddress.LocalPath, authMethod);
            return new Uri(address);
        }
    }
}
