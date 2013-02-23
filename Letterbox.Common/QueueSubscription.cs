using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Common;

namespace Letterbox.Common.Subscriptions
{
    public class QueueSubscription<T>
    {
        public string QueueName { get; set; }
        public IConsumer<T> Consumer { get; set; }
    }
}
