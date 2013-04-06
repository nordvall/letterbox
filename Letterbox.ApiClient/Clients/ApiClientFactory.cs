using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Letterbox.ApiClient.Tokens;
using Letterbox.Clients;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.ApiClient.Clients
{
    /// <summary>
    /// Creates real clients for Micrsoft/Azure Service Bus
    /// </summary>
    public class ApiClientFactory : IClientFactory
    {
        private ApiTokenProviderFactory _tokenProvider;
        private Uri _queueUri;
        ApiQueueValidator _validator;

        public ApiClientFactory(Uri sbUri, Uri httpsUri, NetworkCredential credentials)
        {
            _queueUri = sbUri;
            _tokenProvider = new ApiTokenProviderFactory(httpsUri, credentials);
            _validator = new ApiQueueValidator(httpsUri, _tokenProvider);
        }

        public IReceiveClient CreateSubscriptionClient(string topicName, string subscriptionName)
        {
            _validator.EnsureSubscription(topicName, subscriptionName);

            MessagingFactory factory = GetMessagingFactory();
            
            SubscriptionClient client = factory.CreateSubscriptionClient(topicName, subscriptionName);
            var wrapper = new SubscriptionClientWrapper(client);
            return wrapper;
        }

        public ISendClient CreateTopicClient(string topicName)
        {
            _validator.EnsureTopic(topicName);

            MessagingFactory factory = GetMessagingFactory();
            
            TopicClient client = factory.CreateTopicClient(topicName);
            var wrapper = new TopicClientWrapper(client);
            return wrapper;
        }

        public ISendReceiveClient CreateQueueClient(string queueName)
        {
            _validator.EnsureQueue(queueName);

            MessagingFactory factory = GetMessagingFactory();
            QueueClient client = factory.CreateQueueClient(queueName);
            var wrapper = new QueueClientWrapper(client);
            return wrapper;
        }

        private MessagingFactory GetMessagingFactory()
        {
            TokenProvider tokenProvider = _tokenProvider.GetTokenProvider();
            MessagingFactory messageFactory = MessagingFactory.Create(_queueUri, tokenProvider);
            return messageFactory;
        }
    }
}
