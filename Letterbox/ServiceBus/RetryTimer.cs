using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Letterbox.ServiceBus
{
    public class RetryTimer
    {
        private const int _initialWaitInterval = 2000;
        private const int _maxWaitInterval = 60000;
        private int _currentWaitInterval;

        public RetryTimer()
        {
            Reset();
        }

        public void Reset()
        {
            _currentWaitInterval = _initialWaitInterval;
        }

        public void Wait()
        {
            Thread.Sleep(_currentWaitInterval);

            // Increase wait timer for next round, as long as we do not reach _maxWaitInterval
            if (_currentWaitInterval * 2 < _maxWaitInterval)
            {
                _currentWaitInterval = _currentWaitInterval * 2;
            }
        }
    }
}
