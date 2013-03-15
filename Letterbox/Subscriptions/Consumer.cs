using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;

namespace Letterbox.Subscriptions
{
    public abstract class Consumer<T> : IConsumer where T : class, new()
    {
        public void ConsumeEnvelope(Envelope envelope)
        {
            T message = envelope.GetMessage<T>();
            ConsumeMessage(message);
        }

        public abstract void ConsumeMessage(T message);
    }
}
