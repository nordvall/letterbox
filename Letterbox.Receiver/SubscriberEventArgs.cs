using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Receiver
{
    public class SubscriberEventArgs : EventArgs
    {
        public SubscriberEventType EventType { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }

        public string Message { get; set; }

        public enum SubscriberEventType
        {
            Received,
            Consumed,
            Failed
        }
    }
}
