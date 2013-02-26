using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.Common;
using Letterbox.Receiver.Clients;

namespace Letterbox.Receiver.Subscriptions
{
    /// <summary>
    /// Uses an IClient to poll for messages for one specific queue/topic. 
    /// Calls IConsumer for each message received.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Subscriber<T> : ISubscriber where T : class, new()
    {
        private IConsumer<T> _consumer;
        private IReceiveClient _client;

        public Subscriber(IReceiveClient client, IConsumer<T> consumer)
        {
            _consumer = consumer;
            _client = client;
        }

        public string Name
        {
            get { return _client.Name; }
        }


        public void Subscribe()
        {
            _client.BeginReceive(new AsyncCallback(MessageArrived));
        }

        private void MessageArrived(IAsyncResult result)
        {
            Envelope envelope = null;

            try
            {
                envelope = _client.EndReceive(result);

                OnEnvelopeReceived(envelope);

                if (envelope != null)
                {
                    T message = envelope.GetMessage<T>();
                    _consumer.Consume(message);

                    _client.Complete(envelope.LockToken);

                    OnEnvelopeConsumed(envelope);
                }
            }
            catch(Exception ex)
            {
                if (envelope != null)
                {
                    _client.DeadLetter(envelope.LockToken);
                    //message.Abandon();
                }

                OnEnvelopeFailed(envelope, ex);
            }

            Subscribe();
        }


        public event SubscriberEventHandler EnvelopeReceived;

        private void OnEnvelopeReceived(Envelope envelope)
        {
            if (EnvelopeReceived != null)
            {
                var args = CreateEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Received);
                EnvelopeReceived(args);
            }
        }

        public event SubscriberEventHandler EnvelopeConsumed;

        private void OnEnvelopeConsumed(Envelope envelope)
        {
            if (EnvelopeConsumed != null)
            {
                var args = CreateEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Consumed);
                EnvelopeConsumed(args);
            }
        }

        public event SubscriberEventHandler EnvelopeFailed;

        private void OnEnvelopeFailed(Envelope envelope, Exception ex)
        {
            if (EnvelopeFailed != null)
            {
                var args = CreateEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Consumed);
                args.ErrorMessage = ex.Message;

                EnvelopeFailed(args);
            }
        }

        private SubscriberEventArgs CreateEventArgs(Envelope envelope, SubscriberEventArgs.SubscriberEventType eventType)
        {
            var args = new SubscriberEventArgs();
            args.EventType = eventType;

            if (envelope != null)
            {
                args.MessageId = envelope.MessageId;
                args.Size = envelope.Size;
                args.EnquedTime = envelope.EnqueuedTimeUtc;
            }

            return args;
        }


        public void Unsubscribe()
        {
            _client.Close();
        }
    }
}
