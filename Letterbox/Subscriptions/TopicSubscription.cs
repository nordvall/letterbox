using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Subscriptions
{
    public class TopicSubscription
    {
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public IConsumer Consumer { get; set; }   
    }
}
