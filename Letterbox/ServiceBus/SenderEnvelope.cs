using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.ServiceBus
{
    public class SenderEnvelope
    {
        public SenderEnvelope(object message)
        {
            Message = message;
            Attempts = new List<DateTime>();
        }

        public object Message { get; private set; }
        public List<DateTime> Attempts { get; private set; }
    }
}
