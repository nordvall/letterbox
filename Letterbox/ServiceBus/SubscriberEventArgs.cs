using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.ServiceBus
{
    public class SubscriberEventArgs : EventArgs
    {
        public SubscriberEventType EventType { get; set; }
        public string MessageId { get; set; }
        public DateTime EnquedTime { get; set; }
        public long Size { get; set; }
        public string ErrorMessage { get; set; }

        public enum SubscriberEventType
        {
            Received,
            Consumed,
            Failed
        }
    }
}
