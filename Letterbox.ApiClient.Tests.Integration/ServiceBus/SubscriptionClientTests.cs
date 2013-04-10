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
    public class SubscriptionClientTests
    {
        private string _topicName = Guid.NewGuid().ToString();
        private string _subscriptionName = "subscription1";

        [TestFixtureSetUp]
        public void CreateQueue()
        {
            var topic = new TopicDescription(_topicName);
            
            AuthorizationRule user1Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User1.UserName, new List<AccessRights> { AccessRights.Manage });
            topic.Authorization.Add(user1Rule);

            AuthorizationRule user2Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User2.UserName, new List<AccessRights> { AccessRights.Listen, AccessRights.Send });
            topic.Authorization.Add(user2Rule);

            AuthorizationRule user3Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User3.UserName, new List<AccessRights> { AccessRights.Send });
            topic.Authorization.Add(user3Rule);

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateTopic(topic);
            nsManager.CreateSubscription(_topicName, _subscriptionName);

            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactory();
            TopicClient topicClient = messagingFactory.CreateTopicClient(_topicName);
            topicClient.Send(new BrokeredMessage("test"));
        }

        

        [Test]
        public void Receive_WhenSslErrorOccurs_MessagingCommunicationExceptionIsThrown()
        {
            Uri validCertUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidCertUri = new Uri("sb://localhost:9355/ServiceBusDefaultNamespace");

            TokenProvider tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { validCertUri }, TestUsers.User1);
            MessagingFactory messagingFactory = MessagingFactory.Create(invalidCertUri, tokenProvider);
            SubscriptionClient client = messagingFactory.CreateSubscriptionClient(_topicName, _subscriptionName);

            Assert.Throws<MessagingCommunicationException>(() => client.Receive(new TimeSpan(1000)));
        }

        [Test]
        public void Receive_WhenUserHasListenPermission_ReceiveSucceeds()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User2);
            SubscriptionClient client = messagingFactory.CreateSubscriptionClient(_topicName, _subscriptionName);
            client.Receive(new TimeSpan(1000));
        }

        [Test]
        public void Receive_WhenUserDoesNotHaveListenPermission_UnauthorizedAccessExceptionIsThrown()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User3);
            SubscriptionClient client = messagingFactory.CreateSubscriptionClient(_topicName, _subscriptionName);
            
            Assert.Throws<UnauthorizedAccessException>(() => client.Receive(new TimeSpan(1000)));
        }

        [Test]
        public void Receive_WhenIncorrectCredentialsIsProvided_UnauthorizedAccessExceptionIsThrown()
        {
            var wrongCredential = new NetworkCredential("aaa", "bbb");
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(wrongCredential);
            SubscriptionClient client = messagingFactory.CreateSubscriptionClient(_topicName, _subscriptionName);
            
            Assert.Throws<UnauthorizedAccessException>(() => client.Receive(new TimeSpan(1000)));
        }

        [Test]
        public void Receive_WhenTopicDoesNotExist_MessagingEntityNotFoundExceptionIsThrown()
        {
            string subscriptionName = Guid.NewGuid().ToString();
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User1);
            SubscriptionClient client = messagingFactory.CreateSubscriptionClient("aaa", _subscriptionName);

            Assert.Throws<MessagingEntityNotFoundException>(() => client.Receive(new TimeSpan(1000)));
        }

        [Test]
        public void Receive_WhenSubscriptionDoesNotExist_MessagingEntityNotFoundExceptionIsThrown()
        {
            string subscriptionName = Guid.NewGuid().ToString();
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User2);
            SubscriptionClient client = messagingFactory.CreateSubscriptionClient(_topicName, "bbb");

            Assert.Throws<MessagingEntityNotFoundException>(() => client.Receive(new TimeSpan(1000)));
        }

        [TestFixtureTearDown]
        public void DeleteTopic()
        {
            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.DeleteTopic(_topicName);
        }
    }
}
