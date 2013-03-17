using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public interface IReceiveClient
    {
        IAsyncResult BeginReceive(AsyncCallback callback);
        Envelope EndReceive(IAsyncResult result);
        Envelope Receive();
        void Close();
        string Name { get; }
        int Timeout { get; set; }
    }
}
