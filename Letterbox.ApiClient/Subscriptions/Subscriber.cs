using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Common;
using Letterbox.Receiver.Clients;
using Microsoft.ServiceBus.Messaging;

namespace Letterbox.Receiver.Subscriptions
{
    /// <summary>
    /// Uses an IClient to poll for messages for one specific queue/topic. 
    /// Calls IConsumer for each message received.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Subscriber<T> : ISubscriber
    {
        private IConsumer<T> _consumer;
        private IClient _client;

        public Subscriber(IClient client, IConsumer<T> consumer)
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
            _client.BeginReceive(new TimeSpan(0, 0, 15), new AsyncCallback(MessageArrived), _client);
        }

        private void MessageArrived(IAsyncResult result)
        {
            BrokeredMessage message = null;

            try
            {
                message = _client.EndReceive(result);

                OnMessageReceived(message);

                if (message != null)
                {
                    T inner = message.GetBody<T>();
                    _consumer.Consume(inner);

                    message.Complete();

                    OnMessageConsumed(message);
                }
            }
            catch(Exception ex)
            {
                if (message != null)
                {
                    message.DeadLetter();
                    //message.Abandon();
                }

                OnMessageFailed(message, ex);
            }

            Subscribe();
        }


        public event SubscriberEventHandler MessageReceived;

        private void OnMessageReceived(BrokeredMessage message)
        {
            if (MessageReceived != null)
            {
                var args = CreateEventArgs(message, SubscriberEventArgs.SubscriberEventType.Received);
                MessageReceived(this, args);
            }
        }

        public event SubscriberEventHandler MessageConsumed;

        private void OnMessageConsumed(BrokeredMessage message)
        {
            if (MessageConsumed != null)
            {
                var args = CreateEventArgs(message, SubscriberEventArgs.SubscriberEventType.Consumed);
                MessageConsumed(this, args);
            }
        }

        public event SubscriberEventHandler MessageFailed;

        private void OnMessageFailed(BrokeredMessage message, Exception ex)
        {
            if (MessageFailed != null)
            {
                var args = CreateEventArgs(message, SubscriberEventArgs.SubscriberEventType.Consumed);
                args.ErrorMessage = ex.Message;

                MessageFailed(this, args);
            }
        }

        private SubscriberEventArgs CreateEventArgs(BrokeredMessage message, SubscriberEventArgs.SubscriberEventType eventType)
        {
            var args = new SubscriberEventArgs();
            args.EventType = eventType;

            if (message != null)
            {
                args.MessageId = message.MessageId;
                args.Size = message.Size;
                args.EnquedTime = message.EnqueuedTimeUtc;
            }

            return args;
        }


        public void Unsubscribe()
        {
            _client.Close();
        }
    }
}
