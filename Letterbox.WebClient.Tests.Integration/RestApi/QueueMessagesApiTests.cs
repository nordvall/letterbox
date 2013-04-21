using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Letterbox.WebClient.Clients;
using Letterbox.WebClient.Web;
using NUnit.Framework;

namespace Letterbox.WebClient.Tests.Integration.RestApi
{
    [TestFixture]
    public class QueueMessagesApiTests
    {
        private UriCreator _uriCreator;
        private readonly Uri _queueUri;
        private WebClientWrapper _webClient;
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";

        public QueueMessagesApiTests()
        {
            Uri serviceBusUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            _uriCreator = new UriCreator(serviceBusUri);
            _webClient =  new WebClientWrapper();

            string queueName = Guid.NewGuid().ToString();
            _queueUri = _uriCreator.GenerateQueueUri(queueName);
        }

        [TestFixtureSetUp]
        public void CreateQueue()
        {
            var rule1 = new AccessRule { UserName = TestUsers.User1.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule1.Permissions.Add(AccessRight.Listen);
            rule1.Permissions.Add(AccessRight.Send);

            var rule2 = new AccessRule { UserName = TestUsers.User2.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule2.Permissions.Add(AccessRight.Listen);

            var rule3 = new AccessRule { UserName = TestUsers.User3.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule3.Permissions.Add(AccessRight.Send);

            var rules = new List<AccessRule> { rule1, rule2, rule3 };

            XElement queueDescription = ObjectDescriptionBuilder.CreateQueueDescription(rules);
            
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("PUT", _queueUri, queueDescription.ToString());

            var webClient = new WebClientWrapper { ThrowOnHttpError = true };
            HttpWebResponse repsonse = webClient.SendRequest(request);
        }

        [Test]
        public void Send_WhenUserHasSendPermission_Http201IsReturned()
        {
            var url = new Uri(_queueUri, string.Format("{0}/messages", _queueUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public void Send_WhenUserDoesNotHaveSendPermission_Http401IsReturned()
        {
            var url = new Uri(_queueUri, string.Format("{0}/messages", _queueUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void Send_WhenIncorrectCredentialsIsProvided_Http401IsReturned()
        {
            var wrongCredential = new NetworkCredential("aaa", "bbb");
            
            var url = new Uri(_queueUri, string.Format("{0}/messages", _queueUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(wrongCredential);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void Send_WhenQueueDoesNotExist_Http404IsReturned()
        {
            string queueName = Guid.NewGuid().ToString();
            Uri queueUri = _uriCreator.GenerateQueueUri(queueName);
            var url = new Uri(queueUri, string.Format("{0}/messages", queueUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void Receive_WhenUserHasListenPermissionAndMessagesExistInQueue_Http201IsReturned()
        {
            SendMessageToQueue();

            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User1);

            var receiveUrl = new Uri(_queueUri, string.Format("{0}/messages/head?timeout=2", _queueUri.AbsolutePath));
            HttpWebRequest receiveRequest = requestFactory.CreateWebRequest("POST", receiveUrl);
            HttpWebResponse receiveResponse = _webClient.SendRequest(receiveRequest);

            Assert.AreEqual(HttpStatusCode.Created, receiveResponse.StatusCode);
        }

        private void SendMessageToQueue()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            var sendUrl = new Uri(_queueUri, string.Format("{0}/messages", _queueUri.AbsolutePath));
            HttpWebRequest sendRequest = requestFactory.CreateWebRequestWithData("POST", sendUrl, "test");
            HttpWebResponse sendResponse = _webClient.SendRequest(sendRequest);
        }

        [Test]
        public void Receive_WhenUserHasListenPermissionAndMessagesDoNotExistInQueue_Http204IsReturned()
        {
            var url = new Uri(_queueUri, string.Format("{0}/messages/head?timeout=2", _queueUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            HttpWebRequest request = requestFactory.CreateWebRequest("POST", url);
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public void Receive_WhenUserDoesNotHaveListenPermission_Http401IsReturned()
        {
            var url = new Uri(_queueUri, string.Format("{0}/messages/head?timeout=2", _queueUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequest("POST", url);
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void Receive_WhenQueueDoesNotExist_Http404IsReturned()
        {
            string queueName = Guid.NewGuid().ToString();
            Uri queueUri = _uriCreator.GenerateQueueUri(queueName);
            var url = new Uri(queueUri, string.Format("{0}/messages/head?timeout=2", queueUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequest("POST", url);
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void Delete_WhenMessageUriIsCorrect_Http200IsReturned()
        {
            SendMessageToQueue();

            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User1);
            var receiveUrl = new Uri(_queueUri, string.Format("{0}/messages/head?timeout=2", _queueUri.AbsolutePath));

            HttpWebRequest receiveRequest = requestFactory.CreateWebRequest("POST", receiveUrl);
            HttpWebResponse receiveResponse = _webClient.SendRequest(receiveRequest);

            Uri messageUri = new Uri(receiveResponse.Headers[HttpResponseHeader.Location]);
            HttpWebRequest deleteRequest = requestFactory.CreateWebRequest("DELETE", messageUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);

            Assert.AreEqual(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Test]
        public void Delete_WhenMessageUriIsIncorrect_Http404IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User1);

            Uri messageUri = new Uri(_queueUri, string.Format("{0}/messages/{1}/{1}", _queueUri.AbsolutePath, Guid.NewGuid()));
            HttpWebRequest deleteRequest = requestFactory.CreateWebRequest("DELETE", messageUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestFixtureTearDown]
        public void DeleteQueue()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("DELETE", _queueUri);

            var webClient = new WebClientWrapper { ThrowOnHttpError = true };
            HttpWebResponse repsonse = webClient.SendRequest(request);
        }
    }
}
