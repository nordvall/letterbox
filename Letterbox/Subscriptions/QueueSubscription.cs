using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Subscriptions
{
    public class QueueSubscription
    {
        public string QueueName { get; set; }
        public IConsumer Consumer { get; set; }
    }
}
