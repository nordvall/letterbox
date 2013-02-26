using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.ApiClient.Clients;
using Letterbox.Clients;
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

        public IReceiveClient CreateSubscriptionClient(string topicName, string subscriptionName)
        {
            var factory = GetMessagingFactory();
            InitializeTopicAndSubscription(topicName, subscriptionName);
            SubscriptionClient client = factory.CreateSubscriptionClient(topicName, subscriptionName);
            var wrapper = new SubscriptionClientWrapper(client);
            return wrapper;
        }

        public ISendClient CreateTopicClient(string topicName)
        {
            var factory = GetMessagingFactory();
            EnsureTopic(topicName);
            TopicClient client = factory.CreateTopicClient(topicName);
            var wrapper = new TopicClientWrapper(client);
            return wrapper;
        }

        public ISendReceiveClient CreateQueueClient(string queueName)
        {
            var factory = GetMessagingFactory();
            QueueClient client = factory.CreateQueueClient(queueName);
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
