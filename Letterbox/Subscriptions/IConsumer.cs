using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;

namespace Letterbox.Subscriptions
{
    public interface IConsumer
    {
        void ConsumeEnvelope(Envelope envelope);
    }
}
