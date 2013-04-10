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
    public class QueueClientTests
    {
        private string _queueName = Guid.NewGuid().ToString();

        [TestFixtureSetUp]
        public void CreateQueue()
        {
            var queue = new QueueDescription(_queueName);

            AuthorizationRule user1Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User1.UserName, new List<AccessRights> { AccessRights.Listen, AccessRights.Send });
            queue.Authorization.Add(user1Rule);

            AuthorizationRule user2Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User2.UserName, new List<AccessRights> { AccessRights.Listen });
            queue.Authorization.Add(user2Rule);

            AuthorizationRule user3Rule = ServiceBusHelper.CreateAccessRule(TestUsers.User3.UserName, new List<AccessRights> { AccessRights.Send });
            queue.Authorization.Add(user3Rule);
            
            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateQueue(queue);
        }

        [Test]
        public void Send_WhenUserHasSendPermission_SendSucceeds()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User3);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);
            client.Send(new BrokeredMessage("test"));
        }

        [Test]
        public void Send_WhenUserDoesNotHaveSendPermission_UnauthorizedAccessExceptionIsThrown()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User2);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);

            Assert.Throws<UnauthorizedAccessException>(() => client.Send(new BrokeredMessage("test")));
        }

        [Test]
        public void Send_WhenIncorrectCredentialsIsProvided_UnauthorizedAccessExceptionIsThrown()
        {
            var wrongCredential = new NetworkCredential("aaa", "bbb");
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(wrongCredential);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);

            Assert.Throws<UnauthorizedAccessException>(() => client.Send(new BrokeredMessage("test")));
        }

        [Test]
        public void Send_WhenQueueDoesNotExist_MessagingEntityNotFoundExceptionIsThrown()
        {
            string queueName = Guid.NewGuid().ToString();
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User1);
            QueueClient client = messagingFactory.CreateQueueClient(queueName);

            Assert.Throws<MessagingEntityNotFoundException>(() => client.Send(new BrokeredMessage("test")));
        }

        [Test]
        public void Send_WhenSslErrorOccurs_MessagingCommunicationExceptionIsThrown()
        {
            Uri validCertUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidCertUri = new Uri("sb://localhost:9355/ServiceBusDefaultNamespace");
            
            TokenProvider tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { validCertUri }, TestUsers.User1);
            MessagingFactory messagingFactory = MessagingFactory.Create(invalidCertUri, tokenProvider);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);
            
            Assert.Throws<MessagingCommunicationException>(() => client.Send(new BrokeredMessage("test")));
        }

        [Test]
        public void Receive_WhenSslErrorOccurs_MessagingCommunicationExceptionIsThrown()
        {
            Uri validCertUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidCertUri = new Uri("sb://localhost:9355/ServiceBusDefaultNamespace");

            TokenProvider tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { validCertUri }, TestUsers.User1);
            MessagingFactory messagingFactory = MessagingFactory.Create(invalidCertUri, tokenProvider);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);

            Assert.Throws<MessagingCommunicationException>(() => client.Receive(new TimeSpan(1000)));
        }

        [Test]
        public void Receive_WhenUserHasListenPermission_ReceiveSucceeds()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User2);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);
            client.Receive(new TimeSpan(1000));
        }

        [Test]
        public void Receive_WhenUserDoesNotHaveListenPermission_UnauthorizedAccessExceptionIsThrown()
        {
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User3);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);
            
            Assert.Throws<UnauthorizedAccessException>(() => client.Receive(new TimeSpan(1000)));
        }

        [Test]
        public void Receive_WhenIncorrectCredentialsIsProvided_UnauthorizedAccessExceptionIsThrown()
        {
            var wrongCredential = new NetworkCredential("aaa", "bbb");
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(wrongCredential);
            QueueClient client = messagingFactory.CreateQueueClient(_queueName);

            Assert.Throws<UnauthorizedAccessException>(() => client.Receive(new TimeSpan(1000)));
        }

        [Test]
        public void Receive_WhenQueueDoesNotExist_MessagingEntityNotFoundExceptionIsThrown()
        {
            string queueName = Guid.NewGuid().ToString();
            MessagingFactory messagingFactory = ServiceBusHelper.GetMessagingFactoryWithOAuthToken(TestUsers.User1);
            QueueClient client = messagingFactory.CreateQueueClient(queueName);

            Assert.Throws<MessagingEntityNotFoundException>(() => client.Receive(new TimeSpan(1000)));
        }

        [TestFixtureTearDown]
        public void DeleteQueue()
        {
            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.DeleteQueue(_queueName);
        }
    }
}
