﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.Subscriptions;

namespace Letterbox.ServiceBus
{
    /// <summary>
    /// Uses an IClient to poll for messages for one specific queue/topic. 
    /// Calls IConsumer for each message received.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Subscriber : ISubscriber
    {
        private IConsumer _consumer;
        private IReceiveClient _client;

        public Subscriber(IReceiveClient client, IConsumer consumer)
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

                if (envelope != null)
                {
                    OnEnvelopeReceived(envelope);

                    _consumer.ConsumeEnvelope(envelope);

                    envelope.Complete();

                    OnEnvelopeConsumed(envelope);
                }
            }
            catch(Exception ex)
            {
                if (envelope != null)
                {
                    envelope.DeadLetter();
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
                EnvelopeReceived(this, args);
            }
        }

        public event SubscriberEventHandler EnvelopeConsumed;

        private void OnEnvelopeConsumed(Envelope envelope)
        {
            if (EnvelopeConsumed != null)
            {
                var args = CreateEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Consumed);
                EnvelopeConsumed(this, args);
            }
        }

        public event SubscriberEventHandler EnvelopeFailed;

        private void OnEnvelopeFailed(Envelope envelope, Exception ex)
        {
            if (EnvelopeFailed != null)
            {
                var args = CreateEventArgs(envelope, SubscriberEventArgs.SubscriberEventType.Consumed);
                args.ErrorMessage = ex.Message;

                EnvelopeFailed(this, args);
            }
        }

        private SubscriberEventArgs CreateEventArgs(Envelope envelope, SubscriberEventArgs.SubscriberEventType eventType)
        {
            var args = new SubscriberEventArgs();
            args.EventType = eventType;

            if (envelope != null)
            {
                args.MessageId = envelope.MessageId;
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
