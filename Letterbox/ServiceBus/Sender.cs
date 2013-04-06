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
        private TimeSpan _waitForRetry = new TimeSpan(0, 0, 2);
        private ushort _attempts;

        public Sender(ISendClient client, object message)
        {
            _client = client;
            _message = message;

            MaxAttempts = 4;
        }

        public bool Synchronous { get; set; }

        public ushort MaxAttempts { get; set; }

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

        private void HasResponse(IAsyncResult result)
        {
            InvokeWithRetry(() => _client.EndSend(result));
        }

        private void InvokeWithRetry(Action action)
        {
            try
            {
                _attempts++;
                action.Invoke();
            }
            catch (Exception ex)
            {
                if (_attempts < MaxAttempts)
                {
                    Thread.Sleep(_waitForRetry);
                    _waitForRetry = _waitForRetry + _waitForRetry;
                    Send();
                }
                else
                {
                    throw new Exception(string.Format("Message could not be sent in {0} retries", MaxAttempts), ex);
                }
            }
        }
    }
}
