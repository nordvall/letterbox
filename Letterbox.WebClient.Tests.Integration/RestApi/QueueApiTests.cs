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
    public class QueueApiTests
    {
        private UriCreator _uriCreator;
        private readonly Uri _queueUri;
        private WebClientWrapper _webClient;
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";
        private WebRequestFactory _requestFactory;

        public QueueApiTests()
        {
            Uri serviceBusUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            _uriCreator = new UriCreator(serviceBusUri);
            _webClient =  new WebClientWrapper();
            _requestFactory = ServiceBusHelper.GetWebRequestFactory();

            string queueName = Guid.NewGuid().ToString();
            _queueUri = _uriCreator.GenerateQueueUri(queueName);
        }

        [TestFixtureSetUp]
        public void CreateQueue()
        {
            var rule2 = new AccessRule { UserName = TestUsers.User2.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule2.Permissions.Add(AccessRight.Listen);

            var rule3 = new AccessRule { UserName = TestUsers.User3.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule3.Permissions.Add(AccessRight.Send);

            var rules = new List<AccessRule> { rule2, rule3 };

            XElement queueDescription = ObjectDescriptionBuilder.CreateQueueDescription(rules);
            
            HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", _queueUri, queueDescription.ToString());
            
            _webClient.ThrowOnHttpError = true;
            HttpWebResponse response = _webClient.SendRequest(createRequest);
            _webClient.ThrowOnHttpError = false;
        }

        [Test]
        public void GetQueue_WhenUserHasManagePermission_Http200IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _queueUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void GetQueue_WhenQueueDoesNotExist_Http200Isreturned()
        {
            string queueName = Guid.NewGuid().ToString();
            Uri queueUri = _uriCreator.GenerateQueueUri(queueName);
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", queueUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void GetQueue_WhenUserHasListenPermission_Http401IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _queueUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void GetQueue_WhenUserHasSendPermission_Http401IsReturned()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequest("GET", _queueUri);
            HttpWebResponse response = _webClient.SendRequest(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void CreateQueue_WhenQueueAlreadyExists_Http409IsReturned()
        {
            XElement queueDescription = ObjectDescriptionBuilder.CreateQueueDescription(null);
            HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", _queueUri, queueDescription.ToString());
            HttpWebResponse response = _webClient.SendRequest(createRequest);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Test]
        public void CreateQueue_WhenQueueDoesNotExist_Http201IsReturned()
        {
            string queueName = Guid.NewGuid().ToString();
            Uri queueUri = _uriCreator.GenerateQueueUri(queueName);
            XElement queueDescription = ObjectDescriptionBuilder.CreateQueueDescription(null);
            
            try
            {
                HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", queueUri, queueDescription.ToString());
                HttpWebResponse createResponse = _webClient.SendRequest(createRequest);
                Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);
            }
            finally
            {
                HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", queueUri);
                HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);
            }
        }

        [Test]
        public void DeleteQueue_WhenQueueExists_Http200IsReturned()
        {
            string queueName = Guid.NewGuid().ToString();
            Uri queueUri = _uriCreator.GenerateQueueUri(queueName);
            XElement queueDescription = ObjectDescriptionBuilder.CreateQueueDescription(null);

            HttpWebRequest createRequest = _requestFactory.CreateWebRequestWithData("PUT", queueUri, queueDescription.ToString());
            HttpWebResponse createResponse = _webClient.SendRequest(createRequest);
                
            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", queueUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);
            
            Assert.AreEqual(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Test]
        public void DeleteQueue_WhenQueueDoesNotExist_Http404IsReturned()
        {
            string queueName = Guid.NewGuid().ToString();
            Uri queueUri = _uriCreator.GenerateQueueUri(queueName);
            XElement queueDescription = ObjectDescriptionBuilder.CreateQueueDescription(null);

            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", queueUri);
            HttpWebResponse deleteResponse = _webClient.SendRequest(deleteRequest);

            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestFixtureTearDown]
        public void DeleteQueue()
        {
            HttpWebRequest deleteRequest = _requestFactory.CreateWebRequest("DELETE", _queueUri);
            
            _webClient.ThrowOnHttpError = true;
            _webClient.SendRequest(deleteRequest);
            _webClient.ThrowOnHttpError = false;
        }
    }
}
