using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public abstract class Envelope
    {
        public DateTime EnqueuedTimeUtc { get; protected set; }
        public Guid MessageId { get; protected set; }
        public Guid LockToken { get; protected set; }
        public int SequenceNumber { get; protected set; }
        public abstract T GetMessage<T>() where T : class, new();
    }
}
