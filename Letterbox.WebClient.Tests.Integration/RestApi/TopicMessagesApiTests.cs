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
    public class TopicMessagesApiTests
    {
        private UriCreator _uriCreator;
        private readonly Uri _topicUri;
        private WebClientWrapper _webClient;
        private const string _serviceBusNamespace = "ServiceBusDefaultNamespace";

        public TopicMessagesApiTests()
        {
            Uri serviceBusUri = ServiceBusHelper.GetLocalHttpsEndpoint();
            _uriCreator = new UriCreator(serviceBusUri);
            _webClient =  new WebClientWrapper();

            string topicName = Guid.NewGuid().ToString();
            _topicUri = _uriCreator.GenerateTopicUri(topicName);
        }

        [TestFixtureSetUp]
        public void CreateTopic()
        {
            var rule1 = new AccessRule { UserName = TestUsers.User1.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule1.Permissions.Add(AccessRight.Listen);
            rule1.Permissions.Add(AccessRight.Send);

            var rule2 = new AccessRule { UserName = TestUsers.User2.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule2.Permissions.Add(AccessRight.Listen);

            var rule3 = new AccessRule { UserName = TestUsers.User3.UserName, ServiceBusNamespace = _serviceBusNamespace };
            rule3.Permissions.Add(AccessRight.Send);

            var rules = new List<AccessRule> { rule1, rule2, rule3 };

            XElement topicDescription = ObjectDescriptionBuilder.CreateTopicDescription(rules);
            
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("PUT", _topicUri, topicDescription.ToString());

            var webClient = new WebClientWrapper { ThrowOnHttpError = true };
            HttpWebResponse repsonse = webClient.SendRequest(request);
        }

        [Test]
        public void Send_WhenUserHasSendPermission_Http201IsReturned()
        {
            var url = new Uri(_topicUri, string.Format("{0}/messages", _topicUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public void Send_WhenUserDoesNotHaveSendPermission_Http401IsReturned()
        {
            var url = new Uri(_topicUri, string.Format("{0}/messages", _topicUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User2);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void Send_WhenIncorrectCredentialsIsProvided_Http401IsReturned()
        {
            var wrongCredential = new NetworkCredential("aaa", "bbb");
            
            var url = new Uri(_topicUri, string.Format("{0}/messages", _topicUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(wrongCredential);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void Send_WhenTopicDoesNotExist_Http404IsReturned()
        {
            string topicName = Guid.NewGuid().ToString();
            Uri topicUri = _uriCreator.GenerateTopicUri(topicName);
            var url = new Uri(topicUri, string.Format("{0}/messages", topicUri.AbsolutePath));
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactoryWithCredentials(TestUsers.User3);
            HttpWebRequest request = requestFactory.CreateWebRequestWithData("POST", url, "test");
            HttpWebResponse response = _webClient.SendRequest(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestFixtureTearDown]
        public void DeleteTopic()
        {
            WebRequestFactory requestFactory = ServiceBusHelper.GetWebRequestFactory();
            HttpWebRequest request = requestFactory.CreateWebRequest("DELETE", _topicUri);

            var webClient = new WebClientWrapper { ThrowOnHttpError = true };
            HttpWebResponse repsonse = webClient.SendRequest(request);
        }
    }
}
