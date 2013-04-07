using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Letterbox.Clients;
using Letterbox.WebClient.Tokens;
using Letterbox.WebClient.Web;

namespace Letterbox.WebClient.Clients
{
    public class WebQueueValidator : IQueueValidator
    {
        private Uri _managementUri;
        private IWebClient _webClient;
        private WebRequestFactory _requestFactory;
        private UriCreator _uriCreator;
        private const string _createMessage = "<entry xmlns='http://www.w3.org/2005/Atom'><content type='application/xml'><SubscriptionDescription xmlns:i='http://www.w3.org/2001/XMLSchema-instance' xmlns='http://schemas.microsoft.com/netservices/2010/10/servicebus/connect'></SubscriptionDescription></content></entry>";
            
        public WebQueueValidator(Uri managementUri, IWebTokenProvider tokenProvider)
            : this(managementUri, tokenProvider, new WebClientWrapper())
        { }

        public WebQueueValidator(Uri managementUri, IWebTokenProvider tokenProvider, IWebClient webClient)
        {
            _managementUri = managementUri;
            _webClient = webClient;
            _requestFactory = new WebRequestFactory(tokenProvider);
            _uriCreator = new UriCreator(managementUri);
        }

        public void EnsureSubscription(string topicName, string subscriptionName)
        {
            Uri subscriptionUri = _uriCreator.GenerateSubscriptionUri(topicName, subscriptionName);
            
            try
            {
                VerifyServiceBusObject(subscriptionUri);
            }
            catch (ArgumentOutOfRangeException)
            {
                CreateSubscription(subscriptionUri);
            }
        }

        private void CreateSubscription(Uri subscriptionUri)
        {
            HttpWebRequest request = _requestFactory.CreateWebRequestWithData("PUT", subscriptionUri, _createMessage);

            using (HttpWebResponse repsonse = _webClient.SendRequest(request))
            {
                if (repsonse.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception(string.Format("Error verifying service bus object: {0}", subscriptionUri));
                }
            }
        }

        public void EnsureQueue(string queueName)
        {
            Uri queueUri = _uriCreator.GenerateQueueUri(queueName);
            
            VerifyServiceBusObject(queueUri);
        }

        public void EnsureTopic(string topicName)
        {
            Uri topicUri = _uriCreator.GenerateQueueUri(topicName);
            VerifyServiceBusObject(topicUri);
        }

        private void VerifyServiceBusObject(Uri objectUri)
        {
            HttpWebRequest request = _requestFactory.CreateWebRequest("GET", objectUri);

            using (HttpWebResponse repsonse = _webClient.SendRequest(request))
            {
                if (repsonse.StatusCode != HttpStatusCode.OK)
                {
                    switch (repsonse.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            throw new UnauthorizedAccessException(string.Format("Access denied to: {0}", objectUri));
                        case HttpStatusCode.NotFound:
                            throw new ArgumentOutOfRangeException(string.Format("Service bus object does not exist: {0}", objectUri));
                        default:
                            throw new Exception(string.Format("Error verifying service bus object: {0}", objectUri));
                    }
                }
            }
        }
    }
}
