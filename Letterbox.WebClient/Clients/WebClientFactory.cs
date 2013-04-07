using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Letterbox.Clients;
using Letterbox.WebClient.Tokens;
using Letterbox.WebClient.Web;

namespace Letterbox.WebClient.Clients
{
    public class WebClientFactory : IClientFactory
    {
        private Uri _serviceBusAddress;
        private IWebTokenProvider _tokenProvider;
        private WebRequestFactory _requestFactory;
        private UriCreator _uriCreator;

        public WebClientFactory(Uri serviceBusAddress)
            : this(serviceBusAddress, null)
        { }

        public WebClientFactory(Uri serviceBusAddress, NetworkCredential credential)
        {
            _serviceBusAddress = serviceBusAddress;
            _tokenProvider = new WebTokenProvider(serviceBusAddress, credential);
            _requestFactory = new WebRequestFactory(_tokenProvider);
            _uriCreator = new UriCreator(serviceBusAddress);
        }

        public ISendReceiveClient CreateQueueClient(string queueName)
        {
            var url = _uriCreator.GenerateQueueUri(queueName);
            var client = new ServiceBusClient(url, _tokenProvider);

            return client;
        }

        public ISendClient CreateTopicClient(string topicName)
        {
            Uri url = _uriCreator.GenerateTopicUri(topicName);

            return new ServiceBusClient(url, _tokenProvider);
        }

        public IReceiveClient CreateSubscriptionClient(string topicName, string subscriptionName)
        {
            Uri subscriptionUrl = _uriCreator.GenerateSubscriptionUri(topicName, subscriptionName);
            
            return new ServiceBusClient(subscriptionUrl, _tokenProvider);
        }

        public IQueueValidator GetValidator()
        {
            return new WebQueueValidator(_serviceBusAddress, _tokenProvider);
        }
    }
}
