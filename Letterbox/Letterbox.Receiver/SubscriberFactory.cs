using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Common;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver
{
    public class SubscriberFactory
    {
        static string ServerFQDN;
        static int HttpPort = 9355;
        static int TcpPort = 9354;
        static string ServiceNamespace = "ServiceBusDefaultNamespace";

        public static Subscriber<T> CreateSubscription<T>(Subscription<T> subscription)
        {
            var factory = GetMessagingFactory();
            InitializeTopicAndSubscription(subscription.TopicName, subscription.SubscriptionName);
            SubscriptionClient client = factory.CreateSubscriptionClient(subscription.TopicName, subscription.SubscriptionName);
            
            return new Subscriber<T>(client, subscription.Consumer);
        }

        public static Subscriber<T> CreateAndStartSubscription<T>(Subscription<T> subscription)
        {
            var factory = GetMessagingFactory();
            InitializeTopicAndSubscription(subscription.TopicName, subscription.SubscriptionName);
            SubscriptionClient client = factory.CreateSubscriptionClient(subscription.TopicName, subscription.SubscriptionName);
            
            var subscriber = new Subscriber<T>(client, subscription.Consumer);
            subscriber.Subscribe();
            return subscriber;
        }
        
        private static void InitializeTopicAndSubscription(string topicName, string subscriptionName)
        {
            EnsureTopic(topicName);
            EnsureSubscription(topicName, subscriptionName);
        }


        private static MessagingFactory GetMessagingFactory()
        {
            ServerFQDN = System.Net.Dns.GetHostEntry(string.Empty).HostName;

            ServiceBusConnectionStringBuilder connBuilder = new ServiceBusConnectionStringBuilder();
            connBuilder.ManagementPort = HttpPort;
            connBuilder.RuntimePort = TcpPort;
            connBuilder.Endpoints.Add(new UriBuilder() { Scheme = "sb", Host = ServerFQDN, Path = ServiceNamespace }.Uri);
            connBuilder.StsEndpoints.Add(new UriBuilder() { Scheme = "https", Host = ServerFQDN, Port = HttpPort, Path = ServiceNamespace }.Uri);

            MessagingFactory messageFactory = MessagingFactory.CreateFromConnectionString(connBuilder.ToString());
            return messageFactory;
        }

        private static NamespaceManager GetNamespaceManager()
        {
            ServerFQDN = System.Net.Dns.GetHostEntry(string.Empty).HostName;

            ServiceBusConnectionStringBuilder connBuilder = new ServiceBusConnectionStringBuilder();
            connBuilder.ManagementPort = HttpPort;
            connBuilder.RuntimePort = TcpPort;
            connBuilder.Endpoints.Add(new UriBuilder() { Scheme = "sb", Host = ServerFQDN, Path = ServiceNamespace }.Uri);
            connBuilder.StsEndpoints.Add(new UriBuilder() { Scheme = "https", Host = ServerFQDN, Port = HttpPort, Path = ServiceNamespace }.Uri);


            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connBuilder.ToString());
            return namespaceManager;
        }

        public static void EnsureTopic(string name)
        {
            NamespaceManager namespaceManager = GetNamespaceManager();
            if (namespaceManager.TopicExists(name) == false)
            {
                namespaceManager.CreateTopic(name);
            }
        }

        public static void EnsureSubscription(string topicName, string subscriptionName)
        {
            NamespaceManager namespaceManager = GetNamespaceManager();
            if (namespaceManager.SubscriptionExists(topicName, subscriptionName) == false)
            {
                namespaceManager.CreateSubscription(topicName, subscriptionName);
            }
        }
    }
}
