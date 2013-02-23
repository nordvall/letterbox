﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Letterbox.WebClient
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

        public string UserName { get; set; }

        public string Password { private get; set; }
        
        public AccessToken GetAccessToken()
        {
            if (_cachedToken == null || _cachedToken.Exprires < DateTime.UtcNow.AddMinutes(5))
            {
                _cachedToken = RequestToken(UserName, Password);
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

            HttpWebRequest request = CreateWebRequest(requestUri, requestContent);

            string tokenValue = _webClient.SendRequest(request);

            var accessToken = new AccessToken(tokenValue);
            return accessToken;
        }


        private Uri GenerateUri()
        {
            string address = string.Format("{0}://{1}:{2}/{3}/$STS/OAuth/", _serviceBusAddress.Scheme, _serviceBusAddress.DnsSafeHost, _serviceBusAddress.Port, _serviceBusAddress.LocalPath);
            return new Uri(address);
        }
        
        private static HttpWebRequest CreateWebRequest(Uri requestUri, string requestContent)
        {
            byte[] body = Encoding.UTF8.GetBytes(requestContent);

            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 1;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = body.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(body, 0, body.Length);
            }

            return request;
        }
    }
}