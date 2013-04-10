using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.ApiClient.Tests.Integration.ServiceBus
{
    class ServiceBusHelper
    {
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";
        private const string _nameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public static NamespaceManager GetNamespaceManagerWithOAuthToken(NetworkCredential credential)
        {
            Uri httpsUri = GetLocalHttpsEndpoint();
            TokenProvider tokenProvider = CreateOAuthTokenProvider(credential);
            
            NamespaceManager nsManager = new NamespaceManager(httpsUri, tokenProvider);
            return nsManager;
        }

        public static NamespaceManager GetNamespaceManager()
        {
            Uri httpsUri = GetLocalHttpsEndpoint();
            TokenProvider tokenProvider = TokenProvider.CreateWindowsTokenProvider(new Uri[] { httpsUri });
            
            NamespaceManager nsManager = new NamespaceManager(httpsUri, tokenProvider);
            return nsManager;
        }

        public static MessagingFactory GetMessagingFactory()
        {
            Uri httpsUri = GetLocalHttpsEndpoint();
            TokenProvider tokenProvider = TokenProvider.CreateWindowsTokenProvider(new Uri[] { httpsUri });
            MessagingFactory messageFactory = CreateMessagingFactory(tokenProvider);
            return messageFactory;
        }

        public static MessagingFactory GetMessagingFactoryWithOAuthToken(NetworkCredential credential)
        {
            TokenProvider tokenProvider = CreateOAuthTokenProvider(credential);
            MessagingFactory messageFactory = CreateMessagingFactory(tokenProvider);
            return messageFactory;
        }

        public static AllowRule CreateAccessRule(string userName, List<AccessRights> accessRights)
        {
            return new AllowRule(_serviceBusNamespace, _nameClaimType, userName, accessRights);
        }

        private static TokenProvider CreateOAuthTokenProvider(NetworkCredential credential)
        {
            Uri httpsUri = GetLocalHttpsEndpoint();
            
            var tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { httpsUri }, credential);
            return tokenProvider;
        }

        private static MessagingFactory CreateMessagingFactory(TokenProvider tokenProvider)
        {
            Uri sbUri = GetLocalTcpEndpoint();

            MessagingFactory messageFactory = MessagingFactory.Create(sbUri, tokenProvider);
            return messageFactory;
        }

        public static Uri GetLocalTcpEndpoint()
        {
            string computername = Environment.GetEnvironmentVariable("computername");
            return new Uri(string.Format("sb://{0}:9354/{1}", computername, _serviceBusNamespace));
        }

        public static Uri GetLocalHttpsEndpoint()
        {
            string computername = Environment.GetEnvironmentVariable("computername");
            return new Uri(string.Format("https://{0}:9355/{1}", computername, _serviceBusNamespace));
        }
    }
}
