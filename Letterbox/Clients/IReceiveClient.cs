using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public interface IReceiveClient
    {
        string Name { get; }
        ushort Timeout { get; set; }
        Envelope Receive();
        void Close();
        void Complete(Envelope envelope);
    }
}
