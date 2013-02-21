using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Common;

namespace Letterbox.Receiver
{
    public class Subscription<T>
    {
        public Subscription()
        {

        }
        public Subscription(string topicName, string subscriptionName, IConsumer<T> consumer)
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
