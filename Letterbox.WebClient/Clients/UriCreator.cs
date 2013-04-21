using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.WebClient.Clients
{
    public class UriCreator
    {
        private Uri _serviceBusUri;
        private const string _apiVersion = "2012-08";

        public UriCreator(Uri serviceBusUri)
        {
            _serviceBusUri = serviceBusUri;
        }

        public Uri GenerateSubscriptionUri(string topicName, string subscriptionName)
        {
            var url = new Uri(_serviceBusUri, string.Format("{0}/{1}/Subscriptions/{2}?api-version={3}", _serviceBusUri.AbsolutePath, topicName, subscriptionName, _apiVersion));
            return url;
        }

        public Uri GenerateTopicUri(string topicName)
        {
            var url = new Uri(_serviceBusUri, string.Format("{0}/{1}?api-version={2}", _serviceBusUri.AbsolutePath, topicName, _apiVersion));
            return url;
        }

        public Uri GenerateQueueUri(string queueName)
        {
            var url = new Uri(_serviceBusUri, string.Format("{0}/{1}?api-version={2}", _serviceBusUri.AbsolutePath, queueName, _apiVersion));
            return url;
        }
    }
}
