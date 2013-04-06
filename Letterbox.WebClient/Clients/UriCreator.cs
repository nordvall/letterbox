using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.WebClient.Clients
{
    public class UriCreator
    {
        private Uri _serviceBusUri;

        public UriCreator(Uri serviceBusUri)
        {
            _serviceBusUri = serviceBusUri;
        }

        public Uri GenerateSubscriptionUri(string topicName, string subscriptionName)
        {
            var url = new Uri(_serviceBusUri, string.Format("{0}/{1}/Subscriptions/{2}", _serviceBusUri.AbsolutePath, topicName, subscriptionName));
            return url;
        }

        public Uri GenerateTopicUri(string topicName)
        {
            var url = new Uri(_serviceBusUri, string.Format("{0}/{1}", _serviceBusUri.AbsolutePath, topicName));
            return url;
        }

        public Uri GenerateQueueUri(string queueName)
        {
            var url = new Uri(_serviceBusUri, string.Format("{0}/{1}", _serviceBusUri.AbsolutePath, queueName));
            return url;
        }
    }
}
