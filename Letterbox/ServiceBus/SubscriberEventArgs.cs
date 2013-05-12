using Letterbox.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.ServiceBus
{
    public class SubscriberEventArgs : EventArgs
    {
        public SubscriberEventArgs()
        { }

        public SubscriberEventArgs(Envelope envelope, SubscriberEventType eventType)
        {
            MessageId = envelope.MessageId;
            EnquedTime = envelope.EnqueuedTimeUtc;
            EventType = eventType;
        }

        public SubscriberEventType EventType { get; set; }
        public Guid MessageId { get; set; }
        public DateTime EnquedTime { get; set; }
        public string ErrorMessage { get; set; }

        public enum SubscriberEventType
        {
            Received,
            Consumed,
            Failed
        }
    }
}
