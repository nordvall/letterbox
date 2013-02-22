using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Receiver
{
    public delegate void SubscriberEventHandler(ISubscriber sender, SubscriberEventArgs e);
}
