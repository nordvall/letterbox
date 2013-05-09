using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public static class IReceiveClientAsyncExtension
    {
        public static IAsyncResult BeginReceive(this IReceiveClient client, AsyncCallback callback)
        {
            Func<Envelope> receiveMethod = client.Receive;
            return receiveMethod.BeginInvoke(callback, receiveMethod);
        }

        public static Envelope EndReceive(this IReceiveClient client, IAsyncResult result)
        {
            Func<Envelope> receiveMethod = result.AsyncState as Func<Envelope>;
            return receiveMethod.EndInvoke(result);
        }
    }
}
