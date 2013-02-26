using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.Receiver.Clients;
using Letterbox.WebClient.Tokens;
using Letterbox.WebClient.Web;

namespace Letterbox.WebClient.Clients
{
    public class WebClientFactory : IClientFactory
    {
        private Uri _serviceBusAddress;
        private ITokenManager _tokenManager;
        private IWebClient _webClient;

        public WebClientFactory(Uri serviceBusAddress, ITokenManager tokenManager)
        {
            _serviceBusAddress = serviceBusAddress;
            _tokenManager = tokenManager;
        }

        public WebClientFactory(Uri serviceBusAddress, ITokenManager tokenManager, IWebClient webClient)
        {
            _serviceBusAddress = serviceBusAddress;
            _tokenManager = tokenManager;
            _webClient = webClient;
        }

        public ISendReceiveClient CreateQueueClient(string queueName)
        {
            var url = new Uri(_serviceBusAddress, string.Format("{0}/{1}", _serviceBusAddress.AbsolutePath, queueName));
            var client = new ServiceBusClient(url, _tokenManager);

            return client;
        }

        public IReceiveClient CreateSubscriptionClient(string topicName, string subscriptionName)
        {
            var url = new Uri(_serviceBusAddress, string.Format("{0}/{1}/Subscriptions/{2}", _serviceBusAddress.AbsolutePath, topicName, subscriptionName));
            return new ServiceBusClient(url, _tokenManager);
        }

        public ISendClient CreateTopicClient(string topicName)
        {
            var url = new Uri(_serviceBusAddress, string.Format("{0}/{1}", _serviceBusAddress.AbsolutePath, topicName));
            return new ServiceBusClient(url, _tokenManager);
        }
    }
}
