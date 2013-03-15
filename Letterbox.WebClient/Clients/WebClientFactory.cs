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
    public class WebClientFactory : IClientFactory
    {
        private Uri _serviceBusAddress;
        private ITokenManager _tokenManager;
        private IWebClient _webClient;

        public WebClientFactory(Uri serviceBusAddress, ITokenManager tokenManager)
        {
            _serviceBusAddress = serviceBusAddress;
            _tokenManager = tokenManager;
            _webClient = new WebClientWrapper();
        }

        public WebClientFactory(Uri serviceBusAddress, ITokenManager tokenManager, IWebClient webClient)
        {
            _serviceBusAddress = serviceBusAddress;
            _tokenManager = tokenManager;
            _webClient = webClient;
        }

        public ISendReceiveClient CreateQueueClient(string queueName)
        {
            var url = GenerateQueueUri(queueName);
            VerifyServiceBusObject(url);

            var client = new ServiceBusClient(url, _tokenManager);

            return client;
        }

        private Uri GenerateQueueUri(string queueName)
        {
            var url = new Uri(_serviceBusAddress, string.Format("{0}/{1}", _serviceBusAddress.AbsolutePath, queueName));
            return url;
        }

        public ISendClient CreateTopicClient(string topicName)
        {
            Uri url = GenerateTopicUri(topicName);
            VerifyServiceBusObject(url);

            return new ServiceBusClient(url, _tokenManager);
        }

        public IReceiveClient CreateSubscriptionClient(string topicName, string subscriptionName)
        {
            var topicUrl = GenerateTopicUri(topicName);
            VerifyServiceBusObject(topicUrl);

            var subscriptionUrl = GenerateSubscriptionUri(topicName, subscriptionName);
            EnsureSubscription(subscriptionUrl);

            return new ServiceBusClient(subscriptionUrl, _tokenManager);
        }

        private Uri GenerateSubscriptionUri(string topicName, string subscriptionName)
        {
            var url = new Uri(_serviceBusAddress, string.Format("{0}/{1}/Subscriptions/{2}", _serviceBusAddress.AbsolutePath, topicName, subscriptionName));
            return url;
        }

        private void VerifyServiceBusObject(Uri objectUri)
        {
            HttpWebRequest request = CreateWebRequest("GET", objectUri);

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

        private void EnsureSubscription(Uri subscriptionUri)
        {
            try 
            {
                VerifyServiceBusObject(subscriptionUri);
            }
            catch(ArgumentOutOfRangeException)
            {
                HttpWebRequest request = CreateWebRequest("PUT", subscriptionUri);
                using (HttpWebResponse repsonse = _webClient.SendRequest(request))
                {
                    if (repsonse.StatusCode != HttpStatusCode.Created)
                    {
                        throw new Exception(string.Format("Error verifying service bus object: {0}", subscriptionUri));
                    }
                }
            }
        }
        
        private Uri GenerateTopicUri(string topicName)
        {
            var url = new Uri(_serviceBusAddress, string.Format("{0}/{1}", _serviceBusAddress.AbsolutePath, topicName));
            return url;
        }

        private HttpWebRequest CreateWebRequest(string httpMethod, Uri requestUri)
        {
            HttpWebRequest request = WebRequest.Create(requestUri) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 1;
            request.Method = httpMethod;
            request.Timeout = 5000;

            AccessToken token = _tokenManager.GetAccessToken();
            string tokenHeaderValue = string.Format("WRAP access_token=\"{0}\"", token.TokenValue);
            request.Headers.Add(HttpRequestHeader.Authorization, tokenHeaderValue);

            return request;
        }
    }
}
