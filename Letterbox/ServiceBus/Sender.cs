using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Letterbox.Clients;

namespace Letterbox.ServiceBus
{
    public class Sender
    {
        private ISendClient _client;
        private TimeSpan _waitForRetry;
        private Queue<object> _sendQueue;
        private object _syncLock = new object();

        public Sender(ISendClient client)
        {
            _client = client;
            _sendQueue = new Queue<object>();
            ResetWaitingTimer();
        }

        private void ResetWaitingTimer()
        {
            _waitForRetry = new TimeSpan(0, 0, 2);
        }

        private void IncreaseWaitingTimer()
        {
            _waitForRetry = _waitForRetry + _waitForRetry;
        }

        public SenderStatus Status { get; private set; }

        public void Send(object message)
        {
            lock (_syncLock)
            {
                _sendQueue.Enqueue(message);

                if (Status == SenderStatus.Idle)
                {
                    TryToSendFirstMessageInQueue();
                }
            }
                
        }

        private void TryToSendFirstMessageInQueue()
        {
            if (_sendQueue.Count > 0)
            {
                object message = _sendQueue.Peek();
                Status = SenderStatus.Sending;
                _client.BeginSend(message, new AsyncCallback(HasResponse));
            }
            else
            {
                Status = SenderStatus.Idle;
            }
        }

        private void HasResponse(IAsyncResult result)
        {
            try
            {
                _client.EndSend(result);

                lock (_syncLock)
                {
                    _sendQueue.Dequeue();
                }

                ResetWaitingTimer();
            }
            catch (Exception ex)
            {
                Status = SenderStatus.WaitForRetry;
                Thread.Sleep(_waitForRetry);
                IncreaseWaitingTimer();
            }

            TryToSendFirstMessageInQueue();
        }

        private void InvokeWithRetry(Action action)
        {
            
        }

        public enum SenderStatus
        {
            Idle,
            Sending,
            WaitForRetry
        }
    }
}
