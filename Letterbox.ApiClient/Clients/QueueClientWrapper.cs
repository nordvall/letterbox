﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.ApiClient.Clients;
using Letterbox.Clients;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver.Clients
{
    public class QueueClientWrapper : ReceiveClientBase, ISendReceiveClient
    {
        QueueClient _client;

        public QueueClientWrapper(QueueClient client)
        {
            _client = client;
            Timeout = 15;
        }

        public string Name
        {
            get { return _client.Path; }
        }

        public override Envelope Receive()
        {
            BrokeredMessage message = _client.Receive();
            if (message == null)
            {
                return null;
            }
            return new ApiClientEnvelope(message);
        }

        public void DeadLetter(Guid lockToken)
        {
            _client.DeadLetter(lockToken);
        }

        public void Defer(Guid lockToken)
        {
            _client.Defer(lockToken);
        }

        public void Abandon(Guid lockToken)
        {
            _client.Abandon(lockToken);
        }

        public void Complete(Guid lockToken)
        {
            _client.Complete(lockToken);
        }

        public void Close()
        {
            _client.Close();
        }

        public void Send(object message)
        {
            var nativeMessage = new BrokeredMessage(message);
            _client.Send(nativeMessage);
        }
    }
}
