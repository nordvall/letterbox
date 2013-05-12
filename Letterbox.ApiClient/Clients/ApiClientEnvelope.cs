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

            EnqueuedTimeUtc = message.EnqueuedTimeUtc;
            MessageId = new Guid(message.MessageId);
            LockToken = message.LockToken;
            SequenceNumber = Convert.ToInt32(message.SequenceNumber);
        }

        public override T GetMessage<T>()
        {
            return _message.GetBody<T>();
        }
    }
}
