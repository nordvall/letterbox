using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver.Clients
{
    public interface IClient
    {
        void BeginReceive(TimeSpan serverWaitTime, AsyncCallback callback, object state);
        BrokeredMessage EndReceive(IAsyncResult result);
        void Close();
        string Name { get; }
    }
}
