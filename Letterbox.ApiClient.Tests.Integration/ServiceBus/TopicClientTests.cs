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
    public class TopicClientTests
    {
        private string _topicName = Guid.NewGuid().ToString();

        [TestFixtureSetUp]
        public void CreateTopic()
        {
            var topic = new TopicDescription(_topicName);

            AuthorizationRule user1Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User1.UserName, new List<AccessRights> { AccessRights.Listen, AccessRights.Send });
            topic.Authorization.Add(user1Rule);

            AuthorizationRule user2Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User2.UserName, new List<AccessRights> { AccessRights.Listen });
            topic.Authorization.Add(user2Rule);

            AuthorizationRule user3Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User3.UserName, new List<AccessRights> { AccessRights.Send });
            topic.Authorization.Add(user3Rule);
            
            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateTopic(topic);
        }

        [Test]
        public void Send_WhenUserHasSendPermission_SendSucceeds()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User3);
            TopicClient client = messagingFactory.CreateTopicClient(_topicName);
            client.Send(new BrokeredMessage("test"));
        }

        [Test]
        public void Send_WhenUserDoesNotHaveSendPermission_UnauthorizedAccessExceptionIsThrown()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User2);
            TopicClient client = messagingFactory.CreateTopicClient(_topicName);

            Assert.Throws<UnauthorizedAccessException>(() => client.Send(new BrokeredMessage("test")));
        }

        [Test]
        public void Send_WhenIncorrectCredentialsIsProvided_UnauthorizedAccessExceptionIsThrown()
        {
            var wrongCredential = new NetworkCredential("aaa", "bbb");
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(wrongCredential);
            TopicClient client = messagingFactory.CreateTopicClient(_topicName);

            Assert.Throws<UnauthorizedAccessException>(() => client.Send(new BrokeredMessage("test")));
        }

        [Test]
        public void Send_WhenTopicDoesNotExist_MessagingEntityNotFoundExceptionIsThrown()
        {
            string topicName = Guid.NewGuid().ToString();
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User1);
            TopicClient client = messagingFactory.CreateTopicClient(topicName);

            Assert.Throws<MessagingEntityNotFoundException>(() => client.Send(new BrokeredMessage("test")));
        }

        [Test]
        public void Send_WhenSslErrorOccurs_MessagingCommunicationExceptionIsThrown()
        {
            Uri validCertUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidCertUri = new Uri("sb://localhost/ServiceBusDefaultNamespace");

            TokenProvider tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { validCertUri }, TestUsers.User1);
            MessagingFactory messagingFactory = MessagingFactory.Create(invalidCertUri, tokenProvider);
            TopicClient client = messagingFactory.CreateTopicClient(_topicName);

            Assert.Throws<MessagingCommunicationException>(() => client.Send(new BrokeredMessage("test")));
        }



        [Test]
        public void Send_WhenUsingWrongPort_MessagingCommunicationExceptionExceptionIsThrown()
        {
            Uri validProtocolUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidPortUri = new Uri("sb://localhost:1111/ServiceBusDefaultNamespace");

            TokenProvider tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { validProtocolUri }, TestUsers.User1);
            MessagingFactory messagingFactory = MessagingFactory.Create(invalidPortUri, tokenProvider);
            TopicClient client = messagingFactory.CreateTopicClient(_topicName);

            Assert.Throws<MessagingCommunicationException>(() => client.Send(new BrokeredMessage("test")));
        }

        [TestFixtureTearDown]
        public void DeleteTopic()
        {
            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.DeleteTopic(_topicName);
        }
    }
}
