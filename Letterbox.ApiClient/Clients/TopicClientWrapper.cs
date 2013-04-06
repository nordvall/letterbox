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

        public IAsyncResult BeginSend(object message, AsyncCallback callback)
        {
            Action<object> sendMethod = Send;
            return sendMethod.BeginInvoke(message, callback, sendMethod);
        }

        public void Send(object message)
        {
            var nativeMessage = new BrokeredMessage(message);
            _client.Send(nativeMessage);
        }

        public void EndSend(IAsyncResult result)
        {
            Action<object> sendMethod = result.AsyncState as Action<object>;
            sendMethod.EndInvoke(result);
        }
    }
}
