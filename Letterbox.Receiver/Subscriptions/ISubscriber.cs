using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Receiver.Subscriptions
{
    public interface ISubscriber
    {
        void Subscribe();
        void Unsubscribe();
        string Name { get; }
        event SubscriberEventHandler MessageReceived;
        event SubscriberEventHandler MessageConsumed;
        event SubscriberEventHandler MessageFailed;
    }
}
