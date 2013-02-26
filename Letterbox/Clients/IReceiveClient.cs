using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public interface IReceiveClient
    {
        void BeginReceive(AsyncCallback callback);
        Envelope EndReceive(IAsyncResult result);
        Envelope Receive();
        void DeadLetter(Guid lockTooken);
        void Defer(Guid lockTooken);
        void Abandon(Guid lockTooken);
        void Complete(Guid lockTooken);
        void Close();
        string Name { get; }
        int Timeout { get; set; }
    }
}
