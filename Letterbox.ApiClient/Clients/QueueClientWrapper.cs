﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.ApiClient.Clients;
using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.ApiClient.Clients
{
    public class QueueClientWrapper : ISendReceiveClient
    {
        QueueClient _client;

        public QueueClientWrapper(QueueClient client)
        {
            _client = client;
            Timeout = 15;
        }

        public ushort Timeout { get; set; }

        public string Name
        {
            get { return _client.Path; }
        }

        public Envelope Receive()
        {
            BrokeredMessage message = null;

            ExceptionGuard.InvokeMethod(() => message = _client.Receive());
            
            if (message == null)
            {
                return null;
            }

            return new ApiClientEnvelope(message);
        }

        public void DeadLetter(Guid lockToken)
        {
            ExceptionGuard.InvokeMethod(() => _client.DeadLetter(lockToken));
        }

        public void Defer(Guid lockToken)
        {
            ExceptionGuard.InvokeMethod(() => _client.Defer(lockToken));
        }

        public void Abandon(Guid lockToken)
        {
            ExceptionGuard.InvokeMethod(() => _client.Abandon(lockToken));
        }

        public void Complete(Guid lockToken)
        {
            ExceptionGuard.InvokeMethod(() => _client.Complete(lockToken));
        }

        public void Close()
        {
            _client.Close();
        }

        public void Send(object message)
        {
            var nativeMessage = new BrokeredMessage(message);
            ExceptionGuard.InvokeMethod(() => _client.Send(nativeMessage));
        }
    }
}
