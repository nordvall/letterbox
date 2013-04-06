using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.ApiClient.Tokens;
using Letterbox.Clients;
using Microsoft.ServiceBus;

namespace Letterbox.ApiClient.Clients
{
    public class ApiQueueValidator
    {
        private Uri _managementUri;
        private ApiTokenProviderFactory _tokenFactory;

        public ApiQueueValidator(Uri managementUri, ApiTokenProviderFactory tokenFactory)
        {
            _managementUri = managementUri;
            _tokenFactory = tokenFactory;
        }

        public void EnsureQueue(string queueName)
        {
            NamespaceManager namespaceManager = GetNamespaceManager();
            if (namespaceManager.QueueExists(queueName) == false)
            {
                namespaceManager.CreateQueue(queueName);
            }
        }

        public void EnsureTopic(string topicName)
        {
            NamespaceManager namespaceManager = GetNamespaceManager();
            if (namespaceManager.TopicExists(topicName) == false)
            {
                namespaceManager.CreateTopic(topicName);
            }
        }

        public void EnsureSubscription(string topicName, string subscriptionName)
        {
            NamespaceManager namespaceManager = GetNamespaceManager();
            if (namespaceManager.SubscriptionExists(topicName, subscriptionName) == false)
            {
                namespaceManager.CreateSubscription(topicName, subscriptionName);
            }
        }

        private NamespaceManager GetNamespaceManager()
        {
            TokenProvider tokenProvider = _tokenFactory.GetTokenProvider();
            NamespaceManager namespaceManager = new NamespaceManager(_managementUri, tokenProvider);
            return namespaceManager;
        }
    }
}
