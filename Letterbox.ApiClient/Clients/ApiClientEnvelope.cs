using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.ApiClient.Clients
{
    public class ApiClientEnvelope : Envelope
    {
        private BrokeredMessage _message;

        public ApiClientEnvelope(BrokeredMessage message)
        {
            _message = message;

            this.EnqueuedTimeUtc = message.EnqueuedTimeUtc;
            this.LockToken = message.LockToken;
            this.MessageId = message.MessageId;
            this.Size = message.Size;
        }

        public override T GetMessage<T>()
        {
            return _message.GetBody<T>();
        }
    }
}
