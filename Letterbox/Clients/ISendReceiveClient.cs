using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public interface ISendReceiveClient : ISendClient, IReceiveClient
    {
    }
}
