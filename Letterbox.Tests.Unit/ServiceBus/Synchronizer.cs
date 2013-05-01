using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Letterbox.Tests.Unit.ServiceBus
{
    public class Synchronizer
    {
        private AutoResetEvent _blocker;
        private ushort _counter;

        public Synchronizer()
        {
            _blocker = new AutoResetEvent(false);
        }

        public void Pulse()
        {
            _counter++;
            _blocker.Set();
        }

        public void Wait(ushort numberOfEvents)
        {
            do
            {
                _blocker.WaitOne();
            } while (_counter < numberOfEvents);
        }
    }
}
