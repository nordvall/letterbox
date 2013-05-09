using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public class ServiceBusCommunicationException : Exception
    {
        public ServiceBusCommunicationException()
        { }

        public ServiceBusCommunicationException(string message)
            : base(message)
        { }

        public ServiceBusCommunicationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
