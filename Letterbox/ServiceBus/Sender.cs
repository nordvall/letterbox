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
        private object _message;
        private ushort _maxRetries = 5;
        private TimeSpan _waitForRetry = new TimeSpan(0, 0, 2);

        public Sender(ISendClient client, object message)
        {
            _client = client;
            _message = message;
        }

        public bool Synchronous { get; set; }

        public bool MessageSent { get; private set; }

        public ushort Attempts { get; private set; }

        public void Send()
        {
            if (Synchronous)
            {
                InvokeWithRetry(() => _client.Send(_message));
            }
            else
            {
                _client.BeginSend(_message, new AsyncCallback(HasResponse));
            }
        }

        private void InvokeWithRetry(Action action)
        {
            while (Attempts <= _maxRetries)
            {
                try
                {
                    Attempts++;
                    action.Invoke();
                    MessageSent = true;
                }
                catch
                {
                    Thread.Sleep(_waitForRetry);
                    _waitForRetry = _waitForRetry + _waitForRetry;
                    Send();
                }
            }

            if (MessageSent == false)
            {
                throw new Exception(string.Format("Message could not be sent in {0} retries", _maxRetries));
            }
        }

        private void HasResponse(IAsyncResult result)
        {
            InvokeWithRetry(() => _client.EndSend(result));
        }

        private void Retry()
        {
            Attempts++;
        }
    }
}
