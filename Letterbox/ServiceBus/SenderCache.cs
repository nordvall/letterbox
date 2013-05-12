using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Letterbox.ServiceBus
{
    public class SenderCache
    {
        private Dictionary<string, Sender> _senders;
        private ReaderWriterLockSlim _senderCacheLock = new ReaderWriterLockSlim();

        public Sender GetSender(string name)
        {
            Sender sender = null;

            _senderCacheLock.EnterReadLock();
            _senders.TryGetValue(name, out sender);
            _senderCacheLock.ExitReadLock();

            return sender;
        }

        public void AddSender(string name, Sender sender)
        {
            _senderCacheLock.EnterWriteLock();

            if (_senders.ContainsKey(name))
            {
                _senders[name] = sender;
            }
            else
            {
                _senders.Add(name, sender);
            }
                
            _senderCacheLock.ExitWriteLock();
        }

        
    }
}
