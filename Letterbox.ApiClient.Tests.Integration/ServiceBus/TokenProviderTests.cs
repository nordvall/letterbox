using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace Letterbox.ApiClient.Tests.Integration.ServiceBus
{
    [TestFixture]
    public class TokenProviderTests
    {
        [Test]
        public void CreateOAuthTokenProvider_WhenIncorrectCredentialsIsProvided_NoExceptionIsThrown()
        {
            Uri httpsUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            var wrongCredential = new NetworkCredential("aaa", "bbb");
            TokenProvider tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { httpsUri }, wrongCredential);
        }

        public class OAuthTokenProviderTests
        {
            
        }
    }
}
