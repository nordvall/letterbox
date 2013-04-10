using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace Letterbox.ApiClient.Tests.Integration.ServiceBus
{
    [TestFixture]
    public class MessagingFactoryTests
    {
        [Test]
        public void Create_WhenUsingWrongProtocol_ArgumentExceptionIsThrown()
        {
            Uri validProtocolUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidProtocolUri = new Uri("https://localhost:9354/ServiceBusDefaultNamespace");

            TokenProvider tokenProvider = TokenProvider.CreateWindowsTokenProvider(new Uri[] { validProtocolUri });
            
            Assert.Throws<ArgumentException>(() => MessagingFactory.Create(invalidProtocolUri, tokenProvider));
        }
    }
}
