using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.ApiClient.Clients
{
    public class TopicClientWrapper : ISendClient
    {
        TopicClient _client;

        public TopicClientWrapper(TopicClient client)
        {
            _client = client;
        }

        public string Name
        {
            get { return _client.Path; }
        }

        public void Send(object message)
        {
            var nativeMessage = new BrokeredMessage(message);
            ExceptionGuard.InvokeMethod(() => _client.Send(nativeMessage));
        }
    }
}
