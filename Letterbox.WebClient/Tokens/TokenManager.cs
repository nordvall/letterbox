using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Letterbox.WebClient.Web;

namespace Letterbox.WebClient.Tokens
{
    public class TokenManager : ITokenManager
    {
        private IWebClient _webClient;
        private Uri _serviceBusAddress;
        private AccessToken _cachedToken;

        public TokenManager(Uri serviceBusAdress)
        {
            _webClient = new WebClientWrapper();
            _serviceBusAddress = serviceBusAdress;
        }

        public TokenManager(Uri serviceBusAdress, IWebClient webClient)
        {
            _webClient = webClient;
            _serviceBusAddress = serviceBusAdress;
        }

        public NetworkCredential Credentials { private get; set; }

        public AccessToken GetAccessToken()
        {
            if (_cachedToken == null || _cachedToken.Exprires < DateTime.UtcNow.AddMinutes(5))
            {
                _cachedToken = RequestToken(Credentials.UserName, Credentials.Password);
            }

            return _cachedToken;
        }
        
        private AccessToken RequestToken(string userName, string userPassword)
        {
            const string ClientPasswordFormat =
                "grant_type=authorization_code&client_id={0}&client_secret={1}&scope={2}";

            Uri requestUri = GenerateUri();
            string requestContent = string.Format(CultureInfo.InvariantCulture,
                ClientPasswordFormat, HttpUtility.UrlEncode(userName),
                HttpUtility.UrlEncode(userPassword),
                HttpUtility.UrlEncode(_serviceBusAddress.AbsoluteUri));
            byte[] data = Encoding.UTF8.GetBytes(requestContent);

            var requestFactory = new WebRequestFactory(this);
            HttpWebRequest request = requestFactory.CreateTokenWebRequest("POST", requestUri, data);

            HttpWebResponse response = _webClient.SendRequest(request);
            using (Stream stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string content = reader.ReadToEnd();
                    var accessToken = new AccessToken(content);
                    return accessToken;
                }
            }
        }


        private Uri GenerateUri()
        {
            string address = string.Format("{0}://{1}:{2}/{3}/$STS/OAuth/", _serviceBusAddress.Scheme, _serviceBusAddress.DnsSafeHost, _serviceBusAddress.Port, _serviceBusAddress.LocalPath);
            return new Uri(address);
        }
    }
}
