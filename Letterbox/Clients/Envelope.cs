using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Letterbox.Clients
{
    public abstract class Envelope
    {
        public DateTime EnqueuedTimeUtc { get; protected set; }
        public Guid LockToken { get; protected set; }
        public string MessageId { get; protected set; }
        public long Size { get; protected set; }
        public abstract T GetMessage<T>() where T : class, new();

        public abstract void DeadLetter();
        public abstract void Defer();
        public abstract void Abandon();
        public abstract void Complete();
    }
}
