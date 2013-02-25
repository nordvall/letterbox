using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Common;

namespace Letterbox.Common.Subscriptions
{
    public class TopicSubscription<T>
    {
        public TopicSubscription()
        {

        }
        public TopicSubscription(string topicName, string subscriptionName, IConsumer<T> consumer)
        {
            TopicName = topicName;
            SubscriptionName = subscriptionName;
            Consumer = consumer;
        }

        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public IConsumer<T> Consumer { get; set; }
    }
}
