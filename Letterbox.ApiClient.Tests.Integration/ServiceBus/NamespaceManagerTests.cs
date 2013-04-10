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
    public class NamespaceManagerTests
    {
        [Test]
        public void CreateAndDeleteQueue_WhenUserHasManagePermission_Success()
        {
            string queueName = Guid.NewGuid().ToString();
            var queue = new QueueDescription(queueName);

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateQueue(queue);
            nsManager.DeleteQueue(queueName);
        }

        [Test]
        public void CreateQueue_WhenUserDoesNotHaveManagePermission_UnauthorizedAccessExceptionIsThrown()
        {
            string queueName = Guid.NewGuid().ToString();
            var queue = new QueueDescription(queueName);

            NamespaceManager nsManager = GetNamespaceManagerWithCustomCredentials(TestUsers.User1);

            Assert.Throws<UnauthorizedAccessException>(() => nsManager.CreateQueue(queue));
        }

        [Test]
        public void CreateQueue_WhenQueueAlreadyExits_MessagingEntityAlreadyExistsExceptionIsThrows()
        {
            string queueName = Guid.NewGuid().ToString();

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateQueue(queueName);

            Assert.Throws<MessagingEntityAlreadyExistsException>(() => nsManager.CreateQueue(queueName));

            nsManager.DeleteQueue(queueName);
        }

        [Test]
        public void DeleteQueue_WhenUserDoesNotHaveManagePermission_UnauthorizedAccessExceptionIsThrown()
        {
            string queueName = Guid.NewGuid().ToString();

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateQueue(queueName);

            NamespaceManager nsManager2 = GetNamespaceManagerWithCustomCredentials(TestUsers.User1);
            Assert.Throws<UnauthorizedAccessException>(() => nsManager2.DeleteQueue(queueName));

            nsManager.DeleteQueue(queueName);
        }

        [Test]
        public void DeleteQueue_WhenQueueDoesNotExist_MessagingEntityNotFoundExceptionIsThrown()
        {
            string queueName = Guid.NewGuid().ToString();

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            
            Assert.Throws<MessagingEntityNotFoundException>(() => nsManager.DeleteQueue(queueName));
        }

        [Test]
        public void CreateAndDeleteTopic_WhenUserHasManagePermission_Success()
        {
            string topicName = Guid.NewGuid().ToString();

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateTopic(topicName);

            nsManager.DeleteTopic(topicName);
        }

        [Test]
        public void CreateTopic_WhenTopicAlreadyExits_MessagingEntityAlreadyExistsExceptionIsThrows()
        {
            string topicName = Guid.NewGuid().ToString();

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateTopic(topicName);

            Assert.Throws<MessagingEntityAlreadyExistsException>(() => nsManager.CreateTopic(topicName));
            
            nsManager.DeleteTopic(topicName);
        }

        [Test]
        public void CreateTopic_WhenUserDoesNotHaveManagePermission_UnauthorizedAccessExceptionIsThrown()
        {
            string topicName = Guid.NewGuid().ToString();
            var topic = new TopicDescription(topicName);

            NamespaceManager nsManager = GetNamespaceManagerWithCustomCredentials(TestUsers.User1);

            Assert.Throws<UnauthorizedAccessException>(() => nsManager.CreateTopic(topic));
        }

        [Test]
        public void DeleteTopic_WhenUserDoesNotHaveManagePermission_UnauthorizedAccessExceptionIsThrown()
        {
            string topicName = Guid.NewGuid().ToString();

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();
            nsManager.CreateTopic(topicName);

            NamespaceManager nsManager2 = GetNamespaceManagerWithCustomCredentials(TestUsers.User1);
            Assert.Throws<UnauthorizedAccessException>(() => nsManager2.DeleteTopic(topicName));

            nsManager.DeleteQueue(topicName);
        }

        [Test]
        public void DeleteTopic_WhenTopicDoesNotExist_MessagingEntityNotFoundExceptionIsThrown()
        {
            string topicName = Guid.NewGuid().ToString();

            NamespaceManager nsManager = ServiceBusHelper.GetNamespaceManager();

            Assert.Throws<MessagingEntityNotFoundException>(() => nsManager.DeleteTopic(topicName));
        }

        [Test]
        public void CreateQueue_WhenSslErrorOccurs_MessagingExceptionExceptionIsThrown()
        {
            string queueName = Guid.NewGuid().ToString();

            Uri validCertUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidCertUri = new Uri("https://localhost:9355/ServiceBusDefaultNamespace");
            TokenProvider tokenProvider = TokenProvider.CreateWindowsTokenProvider(new Uri[] { validCertUri });
            NamespaceManager nsManager = new NamespaceManager(invalidCertUri, tokenProvider);

            Assert.Throws<MessagingException>(() => nsManager.CreateQueue(queueName));
        }

        [Test]
        public void CreateTopic_WhenSslErrorOccurs_MessagingExceptionExceptionIsThrown()
        {
            string topicName = Guid.NewGuid().ToString();

            Uri validCertUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            Uri invalidCertUri = new Uri("https://localhost:9355/ServiceBusDefaultNamespace");
            TokenProvider tokenProvider = TokenProvider.CreateWindowsTokenProvider(new Uri[] { validCertUri });
            NamespaceManager nsManager = new NamespaceManager(invalidCertUri, tokenProvider);

            Assert.Throws<MessagingException>(() => nsManager.CreateTopic(topicName));
        }

        public static NamespaceManager GetNamespaceManagerWithCustomCredentials(NetworkCredential credential)
        {
            Uri httpsUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            TokenProvider tokenProvider = TokenProvider.CreateOAuthTokenProvider(new Uri[] { httpsUri }, credential);
            NamespaceManager nsManager = new NamespaceManager(httpsUri, tokenProvider);
            return nsManager;
        }
    }
}
