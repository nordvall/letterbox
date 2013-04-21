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
    public class SubscriptionApiTests
    {
        private UriCreator _uriCreator;
        private readonly Uri _topicUri;
        private WebClientWrapper _webClient;
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";
        private WebRequestFactory _requestFactory;
        private string _topicName;
        private string _subscriptionName;
        private Uri _subscriptionUri;

        public SubscriptionApiTests()
        {
            Uri serviceBusUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            _uriCreator = new UriCreator(serviceBusUri);
            _webClient =  new WebClientWrapper();
            _requestFactory = ServiceBusHelper.GetWebRequestFactory();

            _topicName = Guid.NewGuid().ToString();
            _topicUri = _uriCreator.GenerateTopicUri(_topicName);

            _subscriptionName = Guid.NewGuid().ToString();
            _subscriptionUri = _uriCreator.GenerateSubscriptionUri(_topicName, _subscriptionName);
        }

        [TestFixtureSetUp]
        public void CreateTopic()
        {
            var rule2 = new AccessRule { UserName = TestUsers.User2.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule2.Permissions.Add(AccessRight.Listen);

            var rule3 = new AccessRule { UserName = TestUsers.User3.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule3.Permissions.Add(AccessRight.Send);

            var rules = new List<AccessRule> { rule2, rule3 };

            XElement topicDescription = ObjectDescriptionBuilder.CreateTopicDescription(rules);
            
            HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", _topicUri, topicDescription.ToString());
            
            _webClient.ThrowOnHttpError = true;
            HttpWebResponse response = _webClient.SendRequest(createRequest);
            _webClient.ThrowOnHttpError = false;
        }

        [Test]
        public void GetSubscription_WhenUserHasManagePermission_Http200IsReturned()
        {
            CreateSubscription();
            
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();

            try
            {
                HttpWebRequest request = requestFactory.CreateWebRequest("GET", _subscriptionUri);
                HttpWebResponse response = _webClient.SendRequest(request);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
            finally
            {
                DeleteSubscription();
            }
        }

        private void CreateSubscription()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            XElement subscriptionDescription = ObjectDescriptionBuilder.CreateSubscriptionDescription(null);
            HttpWebRequest createRequest = requestFactory.CreateWebRequestWithData("PUT", _subscriptionUri, subscriptionDescription.ToString());
            HttpWebResponse createResponse = _webClient.SendRequest(createRequest);
        }

        private void DeleteSubscription()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest deleteRequest = requestFactory.CreateWebRequest("DELETE", _subscriptionUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);
        }

        [Test]
        public void GetSubscription_WhenSubscriptionDoesNotExist_Http404Isreturned()
        {
            string subscriptionName = Guid.NewGuid().ToString();
            Uri subscriptionUri = _uriCreator.GenerateSubscriptionUri(_topicName, subscriptionName);
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", subscriptionUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void GetSubscription_WhenUserHasListenPermission_Http401IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _subscriptionUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void GetSubscription_WhenUserHasSendPermission_Http401IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _subscriptionUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void CreateSubscription_WhenSubscriptionAlreadyExists_Http409IsReturned()
        {
            CreateSubscription();
            
            try
            {
                XElement subscriptionDescription = ObjectDescriptionBuilder.CreateSubscriptionDescription(null);
                HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", _subscriptionUri, subscriptionDescription.ToString());
                HttpWebResponse response = _webClient.SendRequest(createRequest);
                Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            }
            finally
            {
                DeleteSubscription();
            }
        }

        [Test]
        public void CreateSubscription_WhenSubscriptionDoesNotExist_Http201IsReturned()
        {
            
            XElement subscriptionDescription = ObjectDescriptionBuilder.CreateSubscriptionDescription(null);
            
            try
            {
                HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", _subscriptionUri, subscriptionDescription.ToString());
                HttpWebResponse createResponse = _webClient.SendRequest(createRequest);
                Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);
            }
            finally
            {
                HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", _subscriptionUri);
                HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);
            }
        }

        [Test]
        public void DeleteSubscription_WhenSubscriptionExists_Http200IsReturned()
        {
            CreateSubscription();
    
            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", _subscriptionUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);
            
            Assert.AreEqual(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Test]
        public void DeleteSubscription_WhenSubscriptionDoesNotExist_Http404IsReturned()
        {
            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", _subscriptionUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestFixtureTearDown]
        public void DeleteTopic()
        {
            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", _topicUri);
            
            _webClient.ThrowOnHttpError = true;
            _webClient.SendRequest(deleteRequest);
            _webClient.ThrowOnHttpError = false;
        }
    }
}
