using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public static class ISendClientAsyncExtension
    {
        public static IAsyncResult BeginSend(this ISendClient client, object message, AsyncCallback callback)
        {
            Action<object> sendMethod = client.Send;
            return sendMethod.BeginInvoke(message, callback, sendMethod);
        }

        public static void EndSend(this ISendClient client, IAsyncResult result)
        {
            Action<object> sendMethod = result.AsyncState as Action<object>;
            sendMethod.EndInvoke(result);
        }
    }
}
