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
        private Queue<SenderEnvelope> _sendQueue;
        private object _syncLock = new object();

        public Sender(ISendClient client)
        {
            _client = client;
            _sendQueue = new Queue<SenderEnvelope>();
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
                
                OnMessageAttempted(envelope);
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

                ResetWaitingTimer();
                OnMessageSucceeded(envelope);
            }
            catch (Exception ex)
            {
                SenderEnvelope envelope = _sendQueue.Peek();
                OnMessageFailed(envelope, ex);

                Status = SenderStatus.WaitForRetry;

                Thread.Sleep(_waitForRetry);
                IncreaseWaitingTimer();
            }

            TryToSendFirstMessageInQueue();
        }

        public event SenderEventHandler MessageAttempted;

        private void OnMessageAttempted(SenderEnvelope envelope)
        {
            if (MessageAttempted != null)
            {
                var args = CreateEventArgs(envelope, SenderEventArgs.SenderEventType.Attempted);
                MessageAttempted(this, args);
            }
        }

        public event SenderEventHandler MessageSucceeded;

        private void OnMessageSucceeded(SenderEnvelope envelope)
        {
            if (MessageSucceeded != null)
            {
                var args = CreateEventArgs(envelope, SenderEventArgs.SenderEventType.Succeeded);
                MessageSucceeded(this, args);
            }
        }

        public event SenderEventHandler MessageFailed;

        private void OnMessageFailed(SenderEnvelope envelope, Exception ex)
        {
            if (MessageFailed != null)
            {
                var args = CreateEventArgs(envelope, SenderEventArgs.SenderEventType.Failed);
                args.ErrorMessage = ex.Message;

                MessageFailed(this, args);
            }
        }

        private SenderEventArgs CreateEventArgs(SenderEnvelope envelope, SenderEventArgs.SenderEventType eventType)
        {
            var args = new SenderEventArgs()
            {
                EventType = eventType,
                Envelope = envelope
            };

            return args;
        }

        public enum SenderStatus
        {
            Idle,
            Sending,
            WaitForRetry
        }
    }
}
