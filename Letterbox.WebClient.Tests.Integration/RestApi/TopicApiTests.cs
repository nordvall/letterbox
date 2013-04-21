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
    public class TopicApiTests
    {
        private UriCreator _uriCreator;
        private readonly Uri _topicUri;
        private WebClientWrapper _webClient;
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";
        private WebRequestFactory _requestFactory;

        public TopicApiTests()
        {
            Uri serviceBusUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            _uriCreator = new UriCreator(serviceBusUri);
            _webClient =  new WebClientWrapper();
            _requestFactory = ServiceBusHelper.GetWebRequestFactory();

            string topicName = Guid.NewGuid().ToString();
            _topicUri = _uriCreator.GenerateTopicUri(topicName);
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
        public void GetTopic_WhenUserHasManagePermission_Http200IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _topicUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void GetTopic_WhenTopicDoesNotExist_Http200Isreturned()
        {
            string topicName = Guid.NewGuid().ToString();
            Uri topicUri = _uriCreator.GenerateTopicUri(topicName);
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", topicUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void GetTopic_WhenUserHasListenPermission_Http401IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _topicUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void GetTopic_WhenUserHasSendPermission_Http401IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _topicUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void CreateTopic_WhenTopicAlreadyExists_Http409IsReturned()
        {
            XElement topicDescription = ObjectDescriptionBuilder.CreateTopicDescription(null);
            HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", _topicUri, topicDescription.ToString());
            HttpWebResponse response = _webClient.SendRequest(createRequest);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Test]
        public void CreateTopic_WhenTopicDoesNotExist_Http201IsReturned()
        {
            string topicName = Guid.NewGuid().ToString();
            Uri topicUri = _uriCreator.GenerateTopicUri(topicName);
            XElement topicDescription = ObjectDescriptionBuilder.CreateTopicDescription(null);
            
            try
            {
                HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", topicUri, topicDescription.ToString());
                HttpWebResponse createResponse = _webClient.SendRequest(createRequest);
                Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);
            }
            finally
            {
                HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", topicUri);
                HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);
            }
        }

        [Test]
        public void DeleteTopic_WhenTopicExists_Http200IsReturned()
        {
            string topicName = Guid.NewGuid().ToString();
            Uri topicUri = _uriCreator.GenerateTopicUri(topicName);
            XElement topicDescription = ObjectDescriptionBuilder.CreateTopicDescription(null);

            HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", topicUri, topicDescription.ToString());
            HttpWebResponse createResponse = _webClient.SendRequest(createRequest);
                
            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", topicUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);
            
            Assert.AreEqual(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Test]
        public void DeleteTopic_WhenTopicDoesNotExist_Http404IsReturned()
        {
            string topicName = Guid.NewGuid().ToString();
            Uri topicUri = _uriCreator.GenerateTopicUri(topicName);

            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", topicUri);
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
