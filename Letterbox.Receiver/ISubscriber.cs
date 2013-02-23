using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Receiver
{
    public interface ISubscriber
    {
        void Subscribe();
        void Unsubscribe();
        string SubscriptionName { get; }
        string TopicName { get; }
        event SubscriberEventHandler MessageReceived;
        event SubscriberEventHandler MessageConsumed;
        event SubscriberEventHandler MessageFailed;
    }
}
