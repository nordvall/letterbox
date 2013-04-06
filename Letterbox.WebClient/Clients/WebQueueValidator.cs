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
    public class WebQueueValidator
    {
        private Uri _managementUri;
        private IWebClient _webClient;
        private WebRequestFactory _requestFactory;

        public WebQueueValidator(Uri managementUri, IWebTokenProvider tokenProvider)
            : this(managementUri, tokenProvider, new WebClientWrapper())
        { }

        public WebQueueValidator(Uri managementUri, IWebTokenProvider tokenProvider, IWebClient webClient)
        {
            _managementUri = managementUri;
            _webClient = webClient;
            _requestFactory = new WebRequestFactory(tokenProvider);
        }

        public void EnsureSubscription(Uri subscriptionUri)
        {
            try
            {
                VerifyServiceBusObject(subscriptionUri);
            }
            catch (ArgumentOutOfRangeException)
            {
                HttpWebRequest request = _requestFactory.CreateWebRequest("PUT", subscriptionUri);

                using (HttpWebResponse repsonse = _webClient.SendRequest(request))
                {
                    if (repsonse.StatusCode != HttpStatusCode.Created)
                    {
                        throw new Exception(string.Format("Error verifying service bus object: {0}", subscriptionUri));
                    }
                }
            }
        }

        public void EnsureQueue(Uri queueUri)
        {
            VerifyServiceBusObject(queueUri);
        }

        public void EnsureTopic(Uri topicUri)
        {
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
