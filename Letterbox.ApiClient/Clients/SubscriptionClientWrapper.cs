using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver.Clients
{
    public class SubscriptionClientWrapper : IClient
    {
        SubscriptionClient _client;

        public SubscriptionClientWrapper(SubscriptionClient client)
        {
            _client = client;
        }

        public string Name 
        { 
            get { return _client.Name; } 
        }

        public void BeginReceive(TimeSpan serverWaitTime, AsyncCallback callback, object state)
        {
            _client.BeginReceive(serverWaitTime, callback, state);
        }

        public BrokeredMessage EndReceive(IAsyncResult result)
        {
            return _client.EndReceive(result);
        }

        public void Close()
        {
            _client.Close();
        }
    }
}
