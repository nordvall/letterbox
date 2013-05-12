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
    public class SubscriptionMessagesApiTests
    {
        private UriCreator _uriCreator;
        private readonly Uri _topicUri;
        private readonly Uri _subscriptionUri;
        private WebClientWrapper _webClient;
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";
        private const string _subscriptionName = "subscription1";
        private string _topicName;

        public SubscriptionMessagesApiTests()
        {
            Uri serviceBusUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            _uriCreator = new UriCreator(serviceBusUri);
            _webClient =  new WebClientWrapper();

            _topicName = Guid.NewGuid().ToString();
            _topicUri = _uriCreator.GenerateTopicUri(_topicName);
            _subscriptionUri = _uriCreator.GenerateSubscriptionUri(_topicName, _subscriptionName);
        }

        [TestFixtureSetUp]
        public void CreateTopic()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            var webClient = new WebClientWrapper { ThrowOnHttpError = true };
            
            var rule1 = new AccessRule { UserName = TestUsers.User1.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule1.Permissions.Add(AccessRight.Listen);
            rule1.Permissions.Add(AccessRight.Send);

            var rule2 = new AccessRule { UserName = TestUsers.User2.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule2.Permissions.Add(AccessRight.Listen);

            var rule3 = new AccessRule { UserName = TestUsers.User3.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule3.Permissions.Add(AccessRight.Send);

            var rules = new List<AccessRule> { rule1, rule2, rule3 };

            XElement topicDescription = ObjectDescriptionBuilder.CreateTopicDescription(rules);
            
            HttpWebRequest topicRequest = requestFactory.CreateWebRequestWithData("PUT", _topicUri, topicDescription.ToString());
            HttpWebResponse topicRepsonse = webClient.SendRequest(topicRequest);
        }

        [SetUp]
        public void CreateSubscriptionAndSendOneMessage()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            var webClient = new WebClientWrapper { ThrowOnHttpError = true };
            
            XElement subscriptionDescription = ObjectDescriptionBuilder.CreateSubscriptionDescription(null);
            HttpWebRequest subscriptionRequest = requestFactory.CreateWebRequestWithData("PUT", _subscriptionUri, subscriptionDescription.ToString());
            HttpWebResponse subscriptionRepsonse = webClient.SendRequest(subscriptionRequest);
        }

        [Test]
        public void ReceiveAndLock_WhenUserHasListenPermissionAndMessagesExist_Http201IsReturned()
        {
            SendMessageToTopic();

            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User1);
            var receiveUrl = new Uri(_subscriptionUri, string.Format("{0}/messages/head?timeout=2", _subscriptionUri.AbsolutePath));

            HttpWebRequest receiveRequest = requestFactory.CreateWebRequest("POST", receiveUrl);
            HttpWebResponse receiveResponse = _webClient.SendRequest(receiveRequest);

            Assert.AreEqual(HttpStatusCode.Created, receiveResponse.StatusCode);
        }

        private void SendMessageToTopic()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            var sendMessageUrl = new Uri(_topicUri, string.Format("{0}/messages", _topicUri.AbsolutePath));
            HttpWebRequest sendRequest = requestFactory.CreateWebRequestWithData("POST", sendMessageUrl, "test");
            HttpWebResponse sendResponse = _webClient.SendRequest(sendRequest);
        }

        [Test]
        public void ReceiveAndLock_WhenUserHasListenPermissionAndMessagesDoesNotExist_Http204IsReturned()
        {
            var url = new Uri(_subscriptionUri, string.Format("{0}/messages/head?timeout=2", _subscriptionUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            HttpWebRequest request = requestFactory.CreateWebRequest("POST", url);

            // Second time the request should time out and return no message
            HttpWebResponse secondResponse = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.NoContent, secondResponse.StatusCode);
        }

        [Test]
        public void ReceiveAndLock_WhenUserDoesNotHaveListenPermission_Http401IsReturned()
        {
            var url = new Uri(_subscriptionUri, string.Format("{0}/messages/head?timeout=2", _subscriptionUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequest("POST", url);
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void ReceiveAndLock_WhenTopicDoesNotExist_Http404IsReturned()
        {
            string topicName = Guid.NewGuid().ToString();
            Uri subscriptionUri = _uriCreator.GenerateSubscriptionUri(topicName, _subscriptionName);
            var url = new Uri(subscriptionUri, string.Format("{0}/messages/head?timeout=2", subscriptionUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);

            HttpWebRequest request = requestFactory.CreateWebRequest("POST", url);
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void ReceiveAndLock_WhenSubscriptionDoesNotExist_Http404IsReturned()
        {
            string subscriptionName = Guid.NewGuid().ToString();
            Uri subscriptionUri = _uriCreator.GenerateSubscriptionUri(_topicName, subscriptionName);
            var url = new Uri(subscriptionUri, string.Format("{0}/messages/head?timeout=2", subscriptionUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            
            HttpWebRequest request = requestFactory.CreateWebRequest("POST", url);
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void Delete_WhenMessageUriIsCorrect_Http200IsReturned()
        {
            SendMessageToTopic();

            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User1);
            var receiveUrl = new Uri(_subscriptionUri, string.Format("{0}/messages/head?timeout=2", _subscriptionUri.AbsolutePath));

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

            Uri messageUri = new Uri(_subscriptionUri, string.Format("{0}/messages/{1}/{1}", _subscriptionUri.AbsolutePath, Guid.NewGuid()));
            HttpWebRequest deleteRequest = requestFactory.CreateWebRequest("DELETE", messageUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TearDown]
        public void DeleteSubscription()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            var webClient = new WebClientWrapper { ThrowOnHttpError = true };

            HttpWebRequest subscriptionRequest = requestFactory.CreateWebRequest("DELETE", _subscriptionUri);
            HttpWebResponse subscriptionRepsonse = webClient.SendRequest(subscriptionRequest);
        }

        [TestFixtureTearDown]
        public void DeleteQueue()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("DELETE", _topicUri);

            var webClient = new WebClientWrapper { ThrowOnHttpError = true };
            HttpWebResponse repsonse = webClient.SendRequest(request);
        }
    }
}
