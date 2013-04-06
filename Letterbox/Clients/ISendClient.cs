using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public interface ISendClient
    {
        IAsyncResult BeginSend(object message, AsyncCallback callback);
        void Send(object message);
        void EndSend(IAsyncResult result);
    }
}
