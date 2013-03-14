using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.ServiceBus
{
    internal interface ISubscriber
    {
        void Subscribe();
        void Unsubscribe();
        string Name { get; }
        event SubscriberEventHandler EnvelopeReceived;
        event SubscriberEventHandler EnvelopeConsumed;
        event SubscriberEventHandler EnvelopeFailed;
    }
}
