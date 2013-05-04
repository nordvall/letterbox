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
        private RetryTimer _retryTimer;
        private Queue<SenderEnvelope> _sendQueue;
        private object _syncLock = new object();

        public Sender(ISendClient client)
        {
            _client = client;
            _sendQueue = new Queue<SenderEnvelope>();
            _retryTimer = new RetryTimer();
        }

        public SenderStatus Status { get; private set; }

        public int QueueLength 
        { 
            get { return _sendQueue.Count; } 
        }

        public void Send(object message)
        {
            var envelope = new SenderEnvelope(message);

            lock (_syncLock)
            {
                _sendQueue.Enqueue(envelope);

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
                SenderEnvelope envelope = _sendQueue.Peek();
                envelope.Attempts.Add(DateTime.Now);
                
                Status = SenderStatus.Sending;
                _client.BeginSend(envelope.Message, new AsyncCallback(HasResponse));
                
                var eventArgs = new SenderEventArgs(envelope, SenderEventArgs.SenderEventType.Attempted);
                DispatchEvent(MessageAttempted, eventArgs);
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

                SenderEnvelope envelope = null;

                lock (_syncLock)
                {
                    envelope = _sendQueue.Dequeue();
                }

                _retryTimer.Reset();

                var eventArgs = new SenderEventArgs(envelope, SenderEventArgs.SenderEventType.Succeeded);
                DispatchEvent(MessageSucceeded, eventArgs);
            }
            catch (Exception ex)
            {
                Status = SenderStatus.WaitForRetry;

                SenderEnvelope envelope = _sendQueue.Peek();

                var eventArgs = new SenderEventArgs(envelope, SenderEventArgs.SenderEventType.Failed);
                eventArgs.ErrorMessage = ex.Message;
                DispatchEvent(MessageFailed, eventArgs);

                _retryTimer.Wait();
            }

            TryToSendFirstMessageInQueue();
        }

        private void DispatchEvent(SenderEventHandler handler, SenderEventArgs eventArgs)
        {
            if (handler != null)
            {
                handler.Invoke(this, eventArgs);
            }
        }

        public event SenderEventHandler MessageAttempted;
        public event SenderEventHandler MessageSucceeded;
        public event SenderEventHandler MessageFailed;

        public enum SenderStatus
        {
            Idle,
            Sending,
            WaitForRetry
        }
    }
}
