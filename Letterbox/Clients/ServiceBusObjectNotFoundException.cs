using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public class ServiceBusObjectNotFoundException : Exception
    {
        public ServiceBusObjectNotFoundException()
        { }

        public ServiceBusObjectNotFoundException(string message)
            : base(message)
        { }

        public ServiceBusObjectNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
