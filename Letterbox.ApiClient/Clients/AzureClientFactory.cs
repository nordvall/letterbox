using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Common;
using Letterbox.Common.Subscriptions;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver.Clients
{
    /// <summary>
    /// Creates real clients for Micrsoft/Azure Service Bus
    /// </summary>
    public class AzureClientFactory : IClientFactory
    {
        private Uri _stsUri;
        private Uri _queueUri;

        public AzureClientFactory(Uri queueUri, Uri stsUri)
        {
            _queueUri = queueUri;
            _stsUri = stsUri;
        }

        public IClient CreateTopicClient<T>(TopicSubscription<T> subscription)
        {
            var factory = GetMessagingFactory();
            InitializeTopicAndSubscription(subscription.TopicName, subscription.SubscriptionName);
            SubscriptionClient client = factory.CreateSubscriptionClient(subscription.TopicName, subscription.SubscriptionName);
            var wrapper = new SubscriptionClientWrapper(client);
            return wrapper;
        }

        public IClient CreateQueueClient<T>(QueueSubscription<T> subscription)
        {
            var factory = GetMessagingFactory();
            QueueClient client = factory.CreateQueueClient(subscription.QueueName);
            var wrapper = new QueueClientWrapper(client);
            return wrapper;
        }

        private MessagingFactory GetMessagingFactory()
        {
            string connectionString = CreateConnectionString();
            MessagingFactory messageFactory = MessagingFactory.CreateFromConnectionString(connectionString);
            return messageFactory;
        }

        private NamespaceManager GetNamespaceManager()
        {
            string connectionString = CreateConnectionString();
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            return namespaceManager;
        }

        private string CreateConnectionString()
        {
            ServiceBusConnectionStringBuilder connBuilder = new ServiceBusConnectionStringBuilder();
            connBuilder.Endpoints.Add(_queueUri);
            connBuilder.StsEndpoints.Add(_stsUri);
            connBuilder.ManagementPort = 9355;
            connBuilder.RuntimePort = 9354;

            return connBuilder.ToString();
        }

        private void InitializeTopicAndSubscription(string topicName, string subscriptionName)
        {
            EnsureTopic(topicName);
            EnsureSubscription(topicName, subscriptionName);
        }

        public void EnsureTopic(string name)
        {
            NamespaceManager namespaceManager = GetNamespaceManager();
            if (namespaceManager.TopicExists(name) == false)
            {
                namespaceManager.CreateTopic(name);
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
    }
}
