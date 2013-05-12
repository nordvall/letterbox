using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.ApiClient.Clients
{
    public class SubscriptionClientWrapper : IReceiveClient
    {
        SubscriptionClient _client;

        public SubscriptionClientWrapper(SubscriptionClient client)
        {
            _client = client;
            Timeout = 15;
        }

        public ushort Timeout { get; set; }

        public string Name 
        { 
            get { return _client.Name; } 
        }

        public Envelope Receive()
        {
            BrokeredMessage message = _client.Receive(new TimeSpan(0, 0, Timeout));
            if (message == null)
            {
                return null;
            }
            return new ApiClientEnvelope(message);
        }

        public void Complete(Envelope envelope)
        {
            ExceptionGuard.InvokeMethod(() => _client.Complete(envelope.LockToken));
        }

        public void Close()
        {
            _client.Close();
        }
    }
}
