using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public interface IReceiveClient
    {
        Envelope Receive();
        void Close();
        string Name { get; }
        ushort Timeout { get; set; }
    }
}
