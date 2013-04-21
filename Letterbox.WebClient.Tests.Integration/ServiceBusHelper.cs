using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using Letterbox.WebClient.Tokens;
using Letterbox.WebClient.Web;

namespace Letterbox.WebClient.Tests.Integration
{
    public static class ServiceBusHelper
    {
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";
        private const string _nameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public static WebTokenProvider GetTokenProvider()
        {
            return GetTokenProviderWithCredentials(null);
        }

        public static WebTokenProvider GetTokenProviderWithCredentials(NetworkCredential credential)
        {
            Uri httpsUri = GetLocalHttpsEndpoint();
            var provider = new WebTokenProvider(httpsUri, credential);
            return provider;
        }

        public static WebRequestFactory GetWebRequestFactory()
        {
            return GetWebRequestFactoryWithCredentials(null);
        }

        public static WebRequestFactory GetWebRequestFactoryWithCredentials(NetworkCredential credential)
        {
            var tokenProvider = GetTokenProviderWithCredentials(credential);
            var factory = new WebRequestFactory(tokenProvider);
            return factory;
        }

        public static Uri GetLocalHttpsEndpoint()
        {
            string computername = Environment.GetEnvironmentVariable("computername");
            return new Uri(string.Format("https://{0}:9355/{1}", computername, _serviceBusNamespace));
        }
    }
}
