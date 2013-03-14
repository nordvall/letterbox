using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Subscriptions
{
    public class QueueSubscription<T>
    {
        public string QueueName { get; set; }
        public IConsumer<T> Consumer { get; set; }
    }
}
