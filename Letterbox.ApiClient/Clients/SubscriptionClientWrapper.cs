using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.ApiClient.Clients;
using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver.Clients
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

        public IAsyncResult BeginReceive(AsyncCallback callback)
        {
            Func<Envelope> receiveMethod = Receive;
            return receiveMethod.BeginInvoke(callback, receiveMethod);
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

        public Envelope EndReceive(IAsyncResult result)
        {
            Func<Envelope> receiveMethod = result.AsyncState as Func<Envelope>;
            return receiveMethod.EndInvoke(result);
        }

        public void DeadLetter(Guid lockToken)
        {
            _client.DeadLetter(lockToken);
        }

        public void Defer(Guid lockToken)
        {
            _client.Defer(lockToken);
        }

        public void Abandon(Guid lockToken)
        {
            _client.Abandon(lockToken);
        }

        public void Complete(Guid lockToken)
        {
            _client.Complete(lockToken);
        }

        public void Close()
        {
            _client.Close();
        }
    }
}
